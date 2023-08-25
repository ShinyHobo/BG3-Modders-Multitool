/// <summary>
/// For helping with rapidly unpacking all game assets at once.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class PakUnpackHelper
    {
        private List<int> Processes;

        public bool Cancelled = true;

        /// <summary>
        /// Unpacks all the .pak files in the game data directory and places them in a folder next to divine.exe
        /// </summary>
        public Task UnpackAllPakFiles()
        {
            GeneralHelper.WriteToConsole("Unpacking processes starting. This could take a while; please wait for all console processes to close on their own.\n");
            Processes = new List<int>();
            var unpackPath = $"{Directory.GetCurrentDirectory()}\\UnpackedData";
            Directory.CreateDirectory(unpackPath);
            var dataDir = Path.Combine(Directory.GetParent(Properties.Settings.Default.bg3Exe) + "\\", @"..\Data");
            var files = Directory.GetFiles(dataDir, "*.pak").Select(file => Path.GetFullPath(file)).ToList();
            var localizationDir = $"{dataDir}\\Localization";
            if (Directory.Exists(localizationDir))
            {
                files.AddRange(Directory.GetFiles(localizationDir, "*.pak").Select(file => Path.GetFullPath(file)).ToList());
            }
            var pakSelection = new Views.PakSelection(files);
            pakSelection.ShowDialog();
            pakSelection.Closed += (sender, e) => pakSelection.Dispatcher.InvokeShutdown();
            var paks = ((PakSelection)pakSelection.DataContext).PakList.Where(pak => pak.IsSelected).Select(pak => pak.Name).ToList();
            return Task.Run(() =>
            {
                return Task.WhenAll(files.Where(file => paks.Contains(Path.GetFileName(file))).Select(async file =>
                {
                    await RunProcessAsync(file, unpackPath);
                }));
            }).ContinueWith(delegate
            {
                GeneralHelper.WriteToConsole("All unpacking processes finished.\n");
                if (!Cancelled)
                {
                    GeneralHelper.WriteToConsole("Unpacking complete!\n");
                }
            });
        }

        /// <summary>
        /// Creates and runs a process window. Adds the process to a list for cancellation.
        /// </summary>
        /// <param name="file">The file to unpack.</param>
        /// <param name="unpackpath">The folder path to unpack the file to.</param>
        /// <returns></returns>
        private Task<int> RunProcessAsync(string file, string unpackpath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Properties.Settings.Default.divineExe,
                Arguments = $" -g \"bg3\" --action \"extract-package\" --source \"{file}\" --destination \"{unpackpath}\" -l \"all\" --use-package-name"
            };

            var tcs = new TaskCompletionSource<int>();
            var process = new Process {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) => {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            Processes.Add(process.Id);

            return tcs.Task;
        }

        /// <summary>
        /// Force closes all tracked divine.exe unpacking processes.
        /// </summary>
        public void CancelUpacking()
        {
            Cancelled = true;
            if(Processes != null && Processes.Count>0)
            {
                foreach (int process in Processes)
                {
                    if(Process.GetProcesses().Any(x => x.Id == process))
                    {
                        try
                        {
                            var proc = Process.GetProcessById(process);
                            if (!proc.HasExited)
                            {
                                proc.Kill();
                                proc.WaitForExit();
                            }
                        }
                        catch { }// only exception should be "Process with ID #### not found", safe to ignore
                    }
                }
                GeneralHelper.WriteToConsole("Unpacking processes cancelled successfully!\n");
            }
        }

        /// <summary>
        /// Decompresses all decompressable files recursively.
        /// </summary>
        /// <returns>The task with the list of all decompressable files.</returns>
        public static Task<List<string>> DecompressAllConvertableFiles()
        {
            return Task.Run(() =>
            {
                GeneralHelper.WriteToConsole($"Retrieving file list for decompression.\n");
                var fileList = FileHelper.DirectorySearch(@"\\?\" + Path.GetFullPath("UnpackedData"));
                GeneralHelper.WriteToConsole($"Retrived file list. Starting decompression; this could take awhile.\n");
                var defaultPath = @"\\?\" + FileHelper.GetPath("");
                var convertFiles = new List<string>();
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                Parallel.ForEach(fileList, file => {
                    var extension = Path.GetExtension(file);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        if(extension == ".loca")
                        {
                            var convertedFile = FileHelper.Convert(file.Replace(defaultPath, ""), "xml");
                            if (Path.GetExtension(convertedFile) == ".xml")
                            {
                                convertFiles.Add(convertedFile);
                            }
                        } 
                        else
                        {
                            var convertedFile = FileHelper.Convert(file.Replace(defaultPath, ""), "lsx");
                            if (Path.GetExtension(convertedFile) == ".lsx")
                            {
                                convertFiles.Add(convertedFile);
                            }
                        }
                        
                    }
                });
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                fileList.Clear();
                GeneralHelper.WriteToConsole($"Decompression completed in {elapsedTime}.\n");
                return convertFiles;
            });
        }
    }
}
