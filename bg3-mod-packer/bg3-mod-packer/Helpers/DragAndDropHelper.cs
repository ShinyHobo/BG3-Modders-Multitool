/// <summary>
/// Helper for processing drag and drop events
/// </summary>
namespace bg3_mod_packer.Helpers
{
    using bg3_mod_packer.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Security.Cryptography;
    using System.Windows;
    using System.Xml;

    public static class DragAndDropHelper
    {
        /// <summary>
        /// Path to the temp directory to use.
        /// </summary>
        public static string TempFolder = Path.GetTempPath() + "BG3ModPacker";

        /// <summary>
        /// Generates a list of meta.lsx files, representing the mods present.
        /// </summary>
        /// <param name="pathlist">The list of directories within the /Mods folder.</param>
        /// <returns></returns>
        public static List<string> GetMetalsxList(string[] pathlist)
        {
            var metaList = new List<string>();
            foreach (string mod in pathlist)
            {
                foreach (string file in Directory.GetFiles(mod))
                {
                    if (Path.GetFileName(file).Equals("meta.lsx"))
                    {
                        metaList.Add(file);
                        ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"meta.lsx file found in {mod}.\n";
                    }
                }
            }

            if (metaList.Count == 0)
            {
                // meta.lsx not found, discontinue
                throw new Exception("meta.lsx not found in /Mods/ModName/ as expected. Discontinuing process.\n");
            }
            return metaList;
        }

        /// <summary>
        /// Packs the mod into a .pak using Norbyte's LSLib divine.exe
        /// </summary>
        /// <param name="fullpath">The full path for the source folder.</param>
        /// <param name="destination">The destination path and mod name.</param>
        public static void PackMod(string fullpath, string destination)
        {
            Directory.CreateDirectory(TempFolder);
            var pathToDivine = Properties.Settings.Default.divineExe;
            var divine = @"/c " + pathToDivine + $" -g \"bg3\" --action \"create-package\" --source \"{fullpath}\" --destination \"{destination}\" -l \"all\"";

            // generate .pak files
            var process = new System.Diagnostics.Process();
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = divine,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            process.StartInfo = startInfo;
            process.Start();
            ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        /// <summary>
        /// Creates the metadata info.json file.
        /// </summary>
        /// <param name="destination">The destination for the .json file to be created.</param>
        /// <param name="metaList">The list of meta.lsx file paths.</param>
        public static void GenerateInfoJson(string destination, List<string> metaList)
        {
            var info = new InfoJson();

            // calculate md5 hash of .pak
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(destination))
                {
                    var hash = md5.ComputeHash(stream);
                    info.MD5 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }

            var created = DateTime.Now;
            var mods = new List<MetaLsx>();
            foreach (string meta in metaList)
            {
                // generate info.json section
                XmlDocument doc = new XmlDocument();
                doc.Load(meta);

                var metadata = new MetaLsx
                {
                    Author = doc.SelectSingleNode("//attribute[@id='Author']").Attributes["value"].InnerText,
                    Name = doc.SelectSingleNode("//attribute[@id='Name']").Attributes["value"].InnerText,
                    Description = doc.SelectSingleNode("//attribute[@id='Description']").Attributes["value"].InnerText,
                    Version = doc.SelectSingleNode("//attribute[@id='Version']").Attributes["value"].InnerText,
                    Folder = doc.SelectSingleNode("//attribute[@id='Folder']").Attributes["value"].InnerText,
                    UUID = doc.SelectSingleNode("//attribute[@id='UUID']").Attributes["value"].InnerText,
                    Created = created
                };

                mods.Add(metadata);
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"Metadata for {metadata.Name} created.\n";
            }

            info.Mods = mods;
            var json = JsonConvert.SerializeObject(info);
            File.WriteAllText(TempFolder + @"\info.json", json);
            ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"info.json generated.\n";
        }

        /// <summary>
        /// Generates a .zip file containing the .pak(s) and info.json (contents of the temp directory)
        /// </summary>
        /// <param name="fullpath">The full path to the directory location to create the .zip.</param>
        /// <param name="name">The name to use for the .zip file.</param>
        public static void GenerateZip(string fullpath, string name)
        {
            // save zip next to folder that was dropped
            var parentDir = Directory.GetParent(fullpath);
            var zip = $"{parentDir.ToString()}\\{name}.zip";
            if (File.Exists(zip))
            {
                File.Delete(zip);
            }
            ZipFile.CreateFromDirectory(TempFolder, zip);
            ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"{name}.zip created.\n";
        }

        /// <summary>
        /// Cleans all the files out of the temp directory used.
        /// </summary>
        public static void CleanTempDirectory()
        {
            // cleanup temp folder
            DirectoryInfo di = new DirectoryInfo(TempFolder);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"Temp files cleaned.\n";
        }
    }
}
