/// <summary>
/// Helper for processing drag and drop events
/// </summary>
namespace bg3_mod_packer.Services
{
    using bg3_mod_packer.Models;
    using bg3_mod_packer.ViewModels;
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
        /// <param name="pathlist">The list of directories within the \Mods folder.</param>
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
                throw new Exception("meta.lsx not found in \\Mods\\ModName\\ as expected. Discontinuing process.\n");
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
            var divine = $" -g \"bg3\" --action \"create-package\" --source \"{fullpath}\" --destination \"{destination}\" -l \"all\"";

            // generate .pak files
            var process = new System.Diagnostics.Process();
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pathToDivine,
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
            ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += process.StandardError.ReadToEnd();
            process.WaitForExit();
        }

        /// <summary>
        /// Creates the metadata info.json file.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="metaList">The list of meta.lsx file paths.</param>
        public static void GenerateInfoJson(Dictionary<string, List<string>> metaList)
        {
            var info = new InfoJson {
                Mods = new List<MetaLsx>()
            };
            var created = DateTime.Now;
            
            foreach(KeyValuePair<string, List<string>> modGroup in metaList)
            {
                var mods = new List<MetaLsx>();
                foreach (var meta in modGroup.Value)
                {
                    // generate info.json section
                    XmlDocument doc = new XmlDocument();
                    doc.Load(meta);

                    var moduleInfo = doc.SelectSingleNode("//node[@id='ModuleInfo']");
                    var metadata = new MetaLsx
                    {
                        Author = moduleInfo.SelectSingleNode("attribute[@id='Author']").Attributes["value"].InnerText,
                        Name = moduleInfo.SelectSingleNode("attribute[@id='Name']").Attributes["value"].InnerText,
                        Description = moduleInfo.SelectSingleNode("attribute[@id='Description']").Attributes["value"].InnerText,
                        Version = moduleInfo.SelectSingleNode("attribute[@id='Version']").Attributes["value"].InnerText,
                        Folder = moduleInfo.SelectSingleNode("attribute[@id='Folder']").Attributes["value"].InnerText,
                        UUID = moduleInfo.SelectSingleNode("attribute[@id='UUID']").Attributes["value"].InnerText,
                        Created = created,
                        Group = modGroup.Key,
                        Dependencies = new List<ModuleShortDesc>()
                    };

                    var dependencies = doc.SelectSingleNode("//node[@id='Dependencies']");
                    if (dependencies != null)
                    {
                        var moduleDescriptions = dependencies.SelectNodes("node[@id='ModuleShortDesc']");
                        foreach (XmlNode moduleDescription in moduleDescriptions)
                        {
                            var depInfo = new ModuleShortDesc
                            {
                                Name = moduleDescription.SelectSingleNode("attribute[@id='Name']").Attributes["value"].InnerText,
                                Version = moduleDescription.SelectSingleNode("attribute[@id='Version']").Attributes["value"].InnerText,
                                Folder = moduleDescription.SelectSingleNode("attribute[@id='Folder']").Attributes["value"].InnerText,
                                UUID = moduleDescription.SelectSingleNode("attribute[@id='UUID']").Attributes["value"].InnerText
                            };
                            metadata.Dependencies.Add(depInfo);
                        }
                    }

                    mods.Add(metadata);
                    ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"Metadata for {metadata.Name} created.\n";
                }

                info.Mods.AddRange(mods);
            }

            // calculate md5 hash of .pak(s)
            using (var md5 = MD5.Create())
            {
                var paks = Directory.GetFiles(TempFolder);
                var pakCount = 1;
                foreach (var pak in paks)
                {
                    byte[] contentBytes = File.ReadAllBytes(pak);
                    if (pakCount == paks.Length)
                        md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                    else
                        md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
                    pakCount++;
                }
                info.MD5 = BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
            }

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

        /// <summary>
        /// Process the file/folder drop.
        /// </summary>
        /// <param name="data">Drop data, should be a folder</param>
        /// <returns>Success</returns>
        public static void ProcessDrop(IDataObject data)
        {
            try
            {
                if (data.GetDataPresent(DataFormats.FileDrop))
                {
                    var fileDrop = data.GetData(DataFormats.FileDrop, true);
                    if (fileDrop is string[] filesOrDirectories && filesOrDirectories.Length > 0)
                    {
                        var mw = Application.Current.MainWindow.DataContext as MainWindow;
                        foreach (string fullPath in filesOrDirectories)
                        {
                            // Only accept directory
                            if (Directory.Exists(fullPath))
                            {
                                var metaList = new Dictionary<string,List<string>>();
                                var dirName = new DirectoryInfo(fullPath).Name;
                                mw.ConsoleOutput += $"Directory name: {dirName}\n";
                                if (Directory.Exists(fullPath + "\\Mods"))
                                {
                                    // single mod directory
                                    metaList.Add(Guid.NewGuid().ToString(), ProcessMod(fullPath, dirName));
                                }
                                else
                                {
                                    // multiple mod directories?
                                    foreach (string dir in Directory.GetDirectories(fullPath))
                                    {
                                        metaList.Add(Guid.NewGuid().ToString(), ProcessMod(dir, new DirectoryInfo(dir).Name));
                                    }
                                }
                                GenerateInfoJson(metaList);
                                GenerateZip(fullPath, dirName);
                                CleanTempDirectory();
                            }
                            else
                            {
                                // File dropping unsupported
                                mw.ConsoleOutput += $"File dropping is not yet supported.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += ex.Message;
            }
        }

        /// <summary>
        /// Process mod for packing.
        /// </summary>
        /// <param name="path">The path to process.</param>
        /// <param name="dirName">The name of the parent directory.</param>
        /// <returns></returns>
        private static List<string> ProcessMod(string path, string dirName)
        {
            var mw = Application.Current.MainWindow.DataContext as MainWindow;
            var destination = TempFolder + $"\\{dirName}.pak";
            mw.ConsoleOutput += $"Destination: {destination}\n";
            mw.ConsoleOutput += $"Attempting to pack mod.\n";
            PackMod(path, destination);
            return GetMetalsxList(Directory.GetDirectories(path + "\\Mods"));
        }
    }
}
