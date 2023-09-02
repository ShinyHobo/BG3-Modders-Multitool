/// <summary>
/// For helping with rapidly unpacking all game assets at once.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Properties;
    using bg3_modders_multitool.ViewModels;
    using LSLib.LS;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    public class PakUnpackHelper : BaseViewModel
    {
        public bool Cancelled;

        private ObservableCollection<PakProgress> _pakProgress;
        public ObservableCollection<PakProgress> PakProgressCollection
        {
            get { return _pakProgress; }
            set { _pakProgress = value; OnNotifyPropertyChanged(); }
        }

        /// <summary>
        /// Unpacks all the .pak files in the game data directory and places them in a folder next to the exe
        /// </summary>
        public Task UnpackAllPakFiles()
        {
            var unpackPath = $"{Directory.GetCurrentDirectory()}\\UnpackedData";
            Directory.CreateDirectory(unpackPath);
            var dataDir = Path.Combine(Directory.GetParent(Properties.Settings.Default.bg3Exe) + "\\", @"..\Data");
            var files = Directory.GetFiles(dataDir, "*.pak", System.IO.SearchOption.AllDirectories).Select(file => Path.GetFullPath(file)).ToList();
            var pakSelection = new Views.PakSelection(files);
            pakSelection.ShowDialog();
            pakSelection.Closed += (sender, e) => pakSelection.Dispatcher.InvokeShutdown();
            var selectedPaks = ((PakSelection)pakSelection.DataContext).PakList.Where(pak => pak.IsSelected).Select(pak => pak.Name).ToList();

            var unpackerProgressWindow = new Views.UnpackerProgress();
            unpackerProgressWindow.DataContext = this;
            unpackerProgressWindow.Show();
            unpackerProgressWindow.Closing += (o, i) => { Cancelled = true; };

            Cancelled = false;
            

            return Task.Run(() =>
            {
                PakProgressCollection = new ObservableCollection<PakProgress>();
                GeneralHelper.WriteToConsole(Properties.Resources.UnpackingProcessStarted);
                var paks = files.Where(file => selectedPaks.Contains(Path.GetFileName(file)));

                foreach (var pak in paks)
                {
                    PakProgressCollection.Add(new PakProgress(Path.GetFileNameWithoutExtension(pak)));
                }

                var cancelError = "Pak unpacking cancelled";
                Parallel.ForEach(paks, new ParallelOptions() { MaxDegreeOfParallelism = 3 }, (pak, loopstate) => {
                    var pakName = Path.GetFileNameWithoutExtension(pak);
                    try
                    {
                        var packager = new Packager();
                        packager.ProgressUpdate = (file2, numerator, denominator, fileInfo) =>
                        {
                            if (Cancelled)
                            {
                                throw new Exception(cancelError);
                            }
                            var newPercent = denominator == 0 ? 0 : (int)(numerator * 100 / denominator);
                            var pakProgress = PakProgressCollection.First(p => p.PakName == pakName);
                            lock (pakProgress)
                                pakProgress.Percent = newPercent;
                        };
                        packager.UncompressPackage(pak, $"{unpackPath}\\{pakName}");

                        Application.Current.Dispatcher.Invoke(() => {
                            lock (PakProgressCollection)
                            {
                                var pakProgress = PakProgressCollection.First(p => p.PakName == pakName);
                                PakProgressCollection.Remove(pakProgress);
                            }
                        });
                        GeneralHelper.WriteToConsole(pakName + " complete");
                    }
                    catch (Exception ex) {
                        if(ex.Message ==  cancelError)
                        {
                            GeneralHelper.WriteToConsole(Properties.Resources.CanceledUnpackingPak, pakName);
                        }
                        else
                        {
                            GeneralHelper.WriteToConsole(Properties.Resources.ErrorUnpacking, pakName, ex.Message);
                        }
                    }
                });
            }).ContinueWith(delegate
            {
                if (Cancelled)
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.UnpackingCancelled);
                }
                else
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.UnpackingProcessComplete);
                    GeneralHelper.WriteToConsole(Properties.Resources.UnpackingComplete);
                }
                Application.Current.Dispatcher.Invoke(() => {
                    unpackerProgressWindow.Close();
                });
            });
        }

        /// <summary>
        /// Decompresses all decompressable files recursively.
        /// </summary>
        /// <returns>The task with the list of all decompressable files.</returns>
        public static Task<List<string>> DecompressAllConvertableFiles()
        {
            return Task.Run(() =>
            {
                GeneralHelper.WriteToConsole(Properties.Resources.RetrievingFileListDecompression);
                var fileList = FileHelper.DirectorySearch(@"\\?\" + Path.GetFullPath("UnpackedData"));
                GeneralHelper.WriteToConsole(Properties.Resources.RetrievedFileListDecompression);
                var defaultPath = @"\\?\" + FileHelper.GetPath("");
                var convertFiles = new List<string>();
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                Parallel.ForEach(fileList, GeneralHelper.ParallelOptions, file => {
                    lock(file)
                    {
                        var extension = Path.GetExtension(file);
                        if (!string.IsNullOrEmpty(extension))
                        {
                            switch (extension)
                            {
                                case ".loca":
                                    {
                                        var convertedFile = FileHelper.Convert(file.Replace(defaultPath, ""), "xml");
                                        if (Path.GetExtension(convertedFile) == ".xml")
                                        {
                                            convertFiles.Add(convertedFile);
                                        }
                                    }
                                    break;
                                case ".xml":
                                    // no conversion necessary
                                    break;
                                default:
                                    {
                                        var convertedFile = FileHelper.Convert(file.Replace(defaultPath, ""), "lsx");
                                        if (Path.GetExtension(convertedFile) == ".lsx")
                                        {
                                            convertFiles.Add(convertedFile);
                                        }
                                    }
                                    break;
                            }

                        }
                    }
                });
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                fileList.Clear();
                GeneralHelper.WriteToConsole(Resources.DecompressionComplete, elapsedTime);
                return convertFiles;
            });
        }

        public class PakProgress : BaseViewModel
        {
            public PakProgress(string pakName)
            {
                PakName = pakName;
                Percent = 0;
            }
            public string PakName { get; set; }
            private int _percent;
            public int Percent
            {
                get { return _percent; }
                set
                {
                    _percent = value;
                    OnNotifyPropertyChanged();
                }
            }
        }
    }
}
