/// <summary>
/// For helping with rapidly unpacking all game assets at once.
/// </summary>
namespace bg3_mod_packer.Services
{
    using bg3_mod_packer.ViewModels;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    public class PakUnpackHelper
    {
        private List<int> Processes;

        public bool Cancelled = true;

        /// <summary>
        /// Unpacks all the .pak files in the game data directory and places them in a folder next to divine.exe
        /// </summary>
        public Task UnpackAllPakFiles()
        {
            var dataContext = Application.Current.MainWindow.DataContext as MainWindow;
            dataContext.ConsoleOutput += "Unpacking processes starting. This could take a while; please wait for all console processes to close on their own.\n";
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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    dataContext.ConsoleOutput += "All unpacking processes finished.\n";
                    if (!Cancelled)
                    {
                        dataContext.ConsoleOutput += "Unpacking complete!\n";
                    }
                });
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
            var dataContext = Application.Current.MainWindow.DataContext as MainWindow;
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
                dataContext.ConsoleOutput += "Unpacking processes cancelled successfully!\n";
            }
        }
    }
}
