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
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    public class PakUnpackHelper : BaseViewModel
    {
        public bool Cancelled;
        public SearchResults DataContext;

        private ObservableCollection<PakProgress> _pakProgress;
        public ObservableCollection<PakProgress> PakProgressCollection
        {
            get { return _pakProgress; }
            set { _pakProgress = value; OnNotifyPropertyChanged(); }
        }

        /// <summary>
        /// Unpacks the selected .pak files in the game data directory and places them in a folder next to the exe
        /// </summary>
        public Task UnpackSelectedPakFiles()
        {
            Directory.CreateDirectory(FileHelper.UnpackedDataPath);
            var dataDir = FileHelper.DataDirectory;
            if (Directory.Exists(dataDir))
            {
                var files = PakReaderHelper.GetPakList();
                var pakSelection = new Views.PakSelection(files);
                pakSelection.ShowDialog();
                pakSelection.Closed += (sender, e) => pakSelection.Dispatcher.InvokeShutdown();
                var selectedPaks = ((PakSelection)pakSelection.DataContext).PakList.Where(pak => pak.IsSelected).Select(pak => pak.Name.Replace("*", "")).ToList();
                var paks = files.Where(file => selectedPaks.Contains(Path.GetFileName(file))).ToList();
                return UnpackPakFiles(paks);
            }
            else
            {
                GeneralHelper.WriteToConsole(Properties.Resources.InvalidBg3Location);
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Unpacks the pak list and displays a progress window
        /// </summary>
        /// <param name="paks">The paks to unpack</param>
        /// <param name="dataFiles">Whether or not the paks are to go into UnpackedData (true) or UnpackedMods (false)</param>
        /// <returns>The task</returns>
        public Task UnpackPakFiles(List<string> paks, bool dataFiles = true)
        {
            var unpackerProgressWindow = new Views.UnpackerProgress();
            unpackerProgressWindow.DataContext = this;
            unpackerProgressWindow.Show();
            unpackerProgressWindow.Closing += (o, i) => { Cancelled = true; };

            Cancelled = false;

            return Task.Run(() =>
            {
                PakProgressCollection = new ObservableCollection<PakProgress>();
                GeneralHelper.WriteToConsole(Properties.Resources.UnpackingProcessStarted);
                

                foreach (var pak in paks)
                {
                    PakProgressCollection.Add(new PakProgress(Path.GetFileNameWithoutExtension(pak)));
                }

                Parallel.ForEach(paks, new ParallelOptions() { MaxDegreeOfParallelism = dataFiles ? 3 : 1 }, (pak, loopstate) => {
                    var pakName = Path.GetFileNameWithoutExtension(pak);
                    try
                    {
                        var packager = new Packager();
                        packager.ProgressUpdate = (file2, numerator, denominator, fileInfo) =>
                        {
                            if (Cancelled)
                            {
                                throw new Exception(Properties.Resources.PakUnpackingCancelled);
                            }
                            var newPercent = denominator == 0 ? 0 : (int)(numerator * 100 / denominator);
                            var pakProgress = PakProgressCollection.First(p => p.PakName == pakName);
                            lock (pakProgress)
                                pakProgress.Percent = newPercent;
                        };

                        if(dataFiles)
                        {
                            packager.UncompressPackage(pak, $"{FileHelper.UnpackedDataPath}\\{pakName}");
                        }
                        else
                        {
                            var tempPath = $"{DragAndDropHelper.TempFolder}\\{pakName}";
                            packager.UncompressPackage(pak, tempPath);
                            var decompressedFileList = DecompressAllConvertableFiles(tempPath);
                            decompressedFileList.Wait();
                            ConvertDecompressedFiles(decompressedFileList.Result, pakName);
                        }

                        Application.Current.Dispatcher.Invoke(() => {
                            lock (PakProgressCollection)
                            {
                                var pakProgress = PakProgressCollection.First(p => p.PakName == pakName);
                                PakProgressCollection.Remove(pakProgress);
                            }
                        });
                        GeneralHelper.WriteToConsole(Properties.Resources.UnpackingPakComplete, pakName);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == Properties.Resources.PakUnpackingCancelled)
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
        /// <param name="path">The file path to decompress</param>
        /// <param name="checking">Whether or not to use the alternate "checking" language</param>
        /// <returns>The task with the list of all files, with decompressed versions replacing the originals.</returns>
        public Task<List<string>> DecompressAllConvertableFiles(string path = null, bool checking = false)
        {
            return Task.Run(() =>
            {
                if(checking)
                    GeneralHelper.WriteToConsole(Properties.Resources.RetrievingFileList);
                else
                    GeneralHelper.WriteToConsole(Properties.Resources.RetrievingFileListDecompression);

                if (DataContext != null)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        DataContext.AllowIndexing = false;
                    });
                }

                path = string.IsNullOrEmpty(path) ? @"\\?\" + FileHelper.UnpackedDataPath : path;
                var fileList = Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories);

                if (DataContext != null)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        DataContext.IsIndexing = true;
                        DataContext.IndexFileTotal = fileList.Length;
                        DataContext.IndexStartTime = DateTime.Now;
                        DataContext.IndexFileCount = 0;
                    });
                }

                if(DataContext?.AllowIndexing == true)
                {
                    GeneralHelper.WriteToConsole(Resources.DecompressionCancelled);
                }

                if(checking)
                    GeneralHelper.WriteToConsole(Properties.Resources.RetrievedFileListDecompressionAlt);
                else
                    GeneralHelper.WriteToConsole(Properties.Resources.RetrievedFileListDecompression);
                
                var defaultPath = @"\\?\" + FileHelper.GetPath("");
                var convertFiles = new ConcurrentBag<string>();
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                Parallel.ForEach(fileList, GeneralHelper.ParallelOptions, (file, loopState) => {
                    if (DataContext?.AllowIndexing != true)
                    {
                        lock (file)
                        {
                            var extension = Path.GetExtension(file);
                            var convertedFile = string.Empty;
                            if (!string.IsNullOrEmpty(extension))
                            {
                                switch (extension)
                                {
                                    case ".loca":
                                        {
                                            convertedFile = FileHelper.Convert(file.Replace(defaultPath, ""), "xml");
                                        }
                                        break;
                                    default:
                                        {
                                            convertedFile = FileHelper.Convert(file.Replace(defaultPath, ""), "lsx");
                                        }
                                        break;
                                }
                                var wasConverted = extension != Path.GetExtension(convertedFile);
                                if (File.Exists(convertedFile))
                                {
                                    convertFiles.Add(convertedFile);
                                    if (file.Contains(FileHelper.UnpackedDataPath) && wasConverted)
                                    {
                                        File.Delete(file);
                                    }
                                }
                                else
                                {
                                    convertFiles.Add(file);
                                }
                            }
                            else
                            {
                                convertFiles.Add(file);
                            }
                            if (DataContext != null)
                            {
                                lock (DataContext)
                                    DataContext.IndexFileCount++;
                            }
                        }
                    }
                    else
                    {
                        loopState.Break();
                    }
                });
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                var wasDecompressing = DataContext == null ? true : !DataContext.AllowIndexing;
                if (DataContext != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DataContext.IsIndexing = false;
                        DataContext.AllowIndexing = true;
                    });
                }

                if(wasDecompressing)
                {
                    GeneralHelper.WriteToConsole(Resources.DecompressionComplete, elapsedTime);
                }
                else
                {
                    GeneralHelper.WriteToConsole(Resources.DecompressionCancelled);
                }

                return convertFiles.ToList();
            });
        }

        /// <summary>
        /// Unpacks the given mod pak path to the unpacked mods directory
        /// </summary>
        /// <param name="pak">The file path pointing to the pak</param>
        public static async void UnpackModToWorkspace(string pak)
        {
            if(File.Exists(pak))
            {
                var pakName = Path.GetFileNameWithoutExtension(pak);
                GeneralHelper.WriteToConsole(Properties.Resources.PakUnpacking, Path.GetFileNameWithoutExtension(pakName));
                var packager = new Packager();
                var unpackPath = $"{FileHelper.UnpackedModsPath}\\{pakName}";
                var tempPath = $"{DragAndDropHelper.TempFolder}\\{pakName}";
                Directory.CreateDirectory(DragAndDropHelper.TempFolder);
                Directory.CreateDirectory(unpackPath);
                var ext = Path.GetExtension(pak);
                if(ext == ".pak")
                {
                    packager.UncompressPackage(pak, tempPath);
                    var decompressedFileList = await new PakUnpackHelper().DecompressAllConvertableFiles(tempPath);
                    ConvertDecompressedFiles(decompressedFileList, pakName);
                }
                else
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.FileTypeNotSupported, ext);
                }
            }
        }

        /// <summary>
        /// Converts the and moves the files into UnpackedMods
        /// </summary>
        /// <param name="decompressedFileList">The file list to convert</param>
        /// <param name="pakName">The pak name</param>
        private static void ConvertDecompressedFiles(List<string> decompressedFileList, string pakName)
        {
            foreach (var file in decompressedFileList)
            {
                var newPath = file.Replace(DragAndDropHelper.TempFolder, FileHelper.UnpackedModsPath);
                new System.IO.FileInfo(newPath).Directory.Create();
                File.Copy(file, newPath, true);
            }
            DragAndDropHelper.CleanTempDirectory();
            GeneralHelper.WriteToConsole(Properties.Resources.PakUnpacked, pakName);
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
