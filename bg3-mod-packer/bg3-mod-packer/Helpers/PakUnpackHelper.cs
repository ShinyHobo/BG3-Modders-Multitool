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
    using System.Windows;

    public class PakUnpackHelper
    {
        List<int> Processes;

        /// <summary>
        /// Unpacks all the .pak files in the game data directory and places them in a folder next to divine.exe
        /// </summary>
        public void UnpackAllPakFiles()
        {
            ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += "Unpacking processes starting. Files found:\n";
            this.Processes = new List<int>();
            var pathToDivine = Properties.Settings.Default.divineExe;
            var unpackPath = $"{pathToDivine}\\..\\UnpackedData";
            Directory.CreateDirectory(unpackPath);
            var dataDir = Path.Combine(Directory.GetParent(Properties.Settings.Default.bg3Exe) + "\\",@"..\Data");
            var files = Directory.GetFiles(dataDir,"*.pak").ToList();
            files.AddRange(Directory.GetFiles($"{dataDir}\\Localization", "*.pak").ToList());
            ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += string.Join("\n",files) + "\n";
            var startInfo = new ProcessStartInfo
            {
                FileName = pathToDivine
            };
            foreach (string file in files)
            {
                var process = new Process();
                startInfo.Arguments =  $" -g \"bg3\" --action \"extract-package\" --source \"{file}\" --destination \"{unpackPath}\" -l \"all\" --use-package-name";
                process.StartInfo = startInfo;
                process.Start();
                Processes.Add(process.Id);
            }
        }

        /// <summary>
        /// Force closes all tracked divine.exe unpacking processes.
        /// </summary>
        public void CancelUpacking()
        {
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
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += "Unpacking processes cancelled successfully\n";
            }
        }
    }
}
