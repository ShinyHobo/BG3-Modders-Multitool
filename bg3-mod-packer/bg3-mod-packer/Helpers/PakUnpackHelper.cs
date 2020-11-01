/// <summary>
/// For helping with rapidly unpacking all game assets at once.
/// </summary>
namespace bg3_mod_packer.Helpers
{
    using bg3_mod_packer.Models;
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
        public async Task UnpackAllPakFiles()
        {
            Application.Current.Dispatcher.Invoke(() => {
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += "Unpacking processes starting.\n";
            });
            Processes = new List<int>();
            var pathToDivine = Properties.Settings.Default.divineExe;
            var unpackPath = $"{Directory.GetCurrentDirectory()}\\UnpackedData";
            Directory.CreateDirectory(unpackPath);
            var dataDir = Path.Combine(Directory.GetParent(Properties.Settings.Default.bg3Exe) + "\\",@"..\Data");
            var files = Directory.GetFiles(dataDir, "*.pak").Select(file=> Path.GetFullPath(file)).ToList();
            var localizationDir = $"{dataDir}\\Localization";
            if (Directory.Exists(localizationDir))
            {
                files.AddRange(Directory.GetFiles(localizationDir, "*.pak").Select(file => Path.GetFullPath(file)).ToList());
            }
            var pakSelection = new Views.PakSelection(files);
            pakSelection.ShowDialog();
            pakSelection.Closed += (sender,e) => pakSelection.Dispatcher.InvokeShutdown();
            var paks = ((PakSelection)pakSelection.DataContext).PakList.Where(pak=>pak.IsSelected).Select(pak=>pak.Name).ToList();
            var startInfo = new ProcessStartInfo
            {
                FileName = pathToDivine
            };
            await Task.WhenAll(files.Where(file => paks.Contains(Path.GetFileName(file))).Select(async file => {
                startInfo.Arguments = $" -g \"bg3\" --action \"extract-package\" --source \"{file}\" --destination \"{unpackPath}\" -l \"all\" --use-package-name";
                await RunProcessAsync(startInfo);
            })).ContinueWith(delegate {
                Application.Current.Dispatcher.Invoke(() => {
                    ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += "Unpacking processes finished.\n";
                });
            });
        }

        private Task<int> RunProcessAsync(ProcessStartInfo startInfo)
        {
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
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += "Unpacking processes cancelled successfully!\n";
            }
        }
    }
}
