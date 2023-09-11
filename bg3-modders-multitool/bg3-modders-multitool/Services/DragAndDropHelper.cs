/// <summary>
/// Helper for processing drag and drop events
/// </summary>
namespace bg3_modders_multitool.Services
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Models;
    using LSLib.LS.Enums;
    using LSLib.LS;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Xml;
    using bg3_modders_multitool.Properties;
    using bg3_modders_multitool.Views.Utilities;

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
                        GeneralHelper.WriteToConsole(Properties.Resources.MetaLsxFound, mod);
                    }
                }
            }

            if (metaList.Count == 0)
            {
                foreach (string mod in pathlist)
                {
                    var invokeResult = Application.Current.Dispatcher.Invoke(() => {
                        var addMeta = new AddMissingMetaLsx(mod);
                        var result = addMeta.ShowDialog();
                        if (result == true)
                        {
                            if (!string.IsNullOrEmpty(addMeta.MetaPath))
                                metaList.Add(addMeta.MetaPath);
                        }
                        return result;
                    });
                }

                if(metaList.Count == 0)
                {
                    // meta.lsx not found, discontinue
                    throw new Exception(Properties.Resources.MetaLsxNotFound);
                }
            }
            return metaList;
        }

        /// <summary>
        /// Packs the mod into a .pak
        /// </summary>
        /// <param name="fullpath">The full path for the source folder.</param>
        /// <param name="destination">The destination path and mod name.</param>
        public static void PackMod(string fullpath, string destination)
        {
            Directory.CreateDirectory(TempFolder);
            var packageOptions = new PackageCreationOptions() { 
                Version = Game.BaldursGate3.PAKVersion(),
                Priority = 21,
                Compression = CompressionMethod.LZ4
            };
            try
            {
                new Packager().CreatePackage(destination, fullpath, packageOptions);
            }
            catch (Exception ex)
            {
                GeneralHelper.WriteToConsole(Properties.Resources.FailedToPackMod, ex.Message);
            }
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
                    var metadata = ReadMeta(meta, created, modGroup);
                    mods.Add(metadata);
                    GeneralHelper.WriteToConsole(Properties.Resources.MetadataCreated, metadata);
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
            GeneralHelper.WriteToConsole(Properties.Resources.InfoGenerated);
        }

        public static MetaLsx ReadMeta(string meta, DateTime? created = null, KeyValuePair<string, List<string>>? modGroup = null)
        {
            // generate info.json section
            XmlDocument doc = new XmlDocument();
            doc.Load(meta);

            var moduleInfo = doc.SelectSingleNode("//node[@id='ModuleInfo']");
            var metadata = new MetaLsx
            {
                Author = moduleInfo.SelectSingleNode("attribute[@id='Author']")?.Attributes["value"].InnerText,
                Name = moduleInfo.SelectSingleNode("attribute[@id='Name']")?.Attributes["value"].InnerText,
                Description = moduleInfo.SelectSingleNode("attribute[@id='Description']")?.Attributes["value"].InnerText,
                Version = moduleInfo.SelectSingleNode("attribute[@id='Version']")?.Attributes["value"].InnerText,
                Folder = moduleInfo.SelectSingleNode("attribute[@id='Folder']")?.Attributes["value"].InnerText,
                UUID = moduleInfo.SelectSingleNode("attribute[@id='UUID']")?.Attributes["value"].InnerText,
                Created = created,
                Group = modGroup?.Key ?? string.Empty,
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
            return metadata;
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
            var zip = $"{parentDir}\\{name}.zip";
            if (File.Exists(zip))
            {
                File.Delete(zip);
            }
            ZipFile.CreateFromDirectory(TempFolder, zip);
            GeneralHelper.WriteToConsole(Properties.Resources.ZipCreated, name);
        }

        /// <summary>
        /// Cleans all the files out of the temp directory used.
        /// </summary>
        public static void CleanTempDirectory()
        {
            // cleanup temp folder
            DirectoryInfo di = new DirectoryInfo(TempFolder);

            foreach (FileInfo file in di.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in di.GetDirectories()) subDirectory.Delete(true);

            GeneralHelper.WriteToConsole(Properties.Resources.TempFilesCleaned);
        }

        /// <summary>
        /// Process the file/folder drop.
        /// </summary>
        /// <param name="data">Drop data, should be a folder</param>
        /// <returns>Success</returns>
        public async static Task ProcessDrop(IDataObject data)
        {
            await Task.Run(() => {
                try
                {
                    if (data.GetDataPresent(DataFormats.FileDrop))
                    {
                        var fileDrop = data.GetData(DataFormats.FileDrop, true);
                        if (fileDrop is string[] filesOrDirectories && filesOrDirectories.Length > 0)
                        {
                            Directory.CreateDirectory(TempFolder);
                            CleanTempDirectory();
                            foreach (string fullPath in filesOrDirectories)
                            {
                                // Only accept directory
                                if (Directory.Exists(fullPath))
                                {
                                    var metaList = new Dictionary<string, List<string>>();
                                    var dirName = new DirectoryInfo(fullPath).Name;
                                    GeneralHelper.WriteToConsole(Properties.Resources.DirectoryName, dirName);
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

                                    if (Properties.Settings.Default.pakToMods)
                                    {
                                        var modsFolder = $"{Properties.Settings.Default.gameDocumentsPath}\\Mods";
                                        if(Directory.Exists(modsFolder))
                                        {
                                            File.Move($"{TempFolder}\\{dirName}.pak", $"{modsFolder}\\{dirName}.pak", MoveOptions.ReplaceExisting);
                                            GeneralHelper.WriteToConsole(Properties.Resources.PakModedToMods, dirName);
                                        }
                                    }
                                    else
                                    {
                                        GenerateInfoJson(metaList);
                                        GenerateZip(fullPath, dirName);
                                    }
                                    CleanTempDirectory();
                                }
                                else if(File.Exists(fullPath))
                                {
                                    var task = Application.Current.Dispatcher.Invoke(() => {
                                        var vm = App.Current.MainWindow.DataContext as ViewModels.MainWindow;
                                        return vm.Unpacker.UnpackPakFiles(new List<string> { fullPath }, false);
                                    });
                                    task.Wait();
                                }
                                else
                                {
                                    GeneralHelper.WriteToConsole(Properties.Resources.FailedToProcessWorkspace);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    GeneralHelper.WriteToConsole(ex.Message);
                    CleanTempDirectory();
                }
            });
        }

        /// <summary>
        /// Builds a pack from converted files.
        /// </summary>
        /// <param name="path">The mod root path.</param>
        /// <returns>The new mod build directory.</returns>
        private static string BuildPack(string path)
        {
            var fileList = FileHelper.DirectorySearch(path);
            var modName = new DirectoryInfo(path).Name;
            var modDir = $"{TempFolder}\\{modName}";
            foreach (var file in fileList)
            {
                var fileParent = file.Replace(path, string.Empty).Replace("\\\\?\\",string.Empty);
                var fileName = Path.GetFileName(file);
                var extension = Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(extension))
                {
                    var conversionFile = fileName.Replace(extension, string.Empty);
                    var secondExtension = Path.GetExtension(conversionFile);
                    // copy to temp dir
                    var mod = $"\\\\?\\{modDir}{fileParent}";
                    var modParent = new DirectoryInfo(mod).Parent.FullName;
                    if (string.IsNullOrEmpty(secondExtension))
                    {
                        if (!Directory.Exists(modParent))
                        {
                            Directory.CreateDirectory(modParent);
                        }
                        if (File.Exists(mod))
                        {
                            File.Delete(mod);
                        }
                        File.Copy(file, mod);
                    }
                    else
                    {
                        // convert and save to temp dir
                        FileHelper.Convert(file, secondExtension.Remove(0, 1), $"{modParent}\\{conversionFile}");
                    }
                }
                else
                {
                    throw new Exception(string.Format(Properties.Resources.FileMissingExtensionError, file));
                }
            }

            ProcessStatsGeneratedDataSubfiles(modDir, modName);

            return modDir;
        }

        /// <summary>
        /// Concatenate Stats\Generated\Data sub directory files
        /// </summary>
        /// <param name="directory">The mod workspace directory</param>
        /// <param name="modName">The mod name to point the search at</param>
        public static void ProcessStatsGeneratedDataSubfiles(string directory, string modName)
        {
            var statsGeneratedDataDir = $"{directory}\\Public\\{modName}\\Stats\\Generated\\Data";
            var sgdInfo = new DirectoryInfo(statsGeneratedDataDir);
            if (sgdInfo.Exists)
            {
                var sgdDirs = sgdInfo.GetDirectories();
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                foreach (var dir in sgdDirs)
                {
                    var files = dir.GetFiles("*.txt", System.IO.SearchOption.AllDirectories);
                    var fileName = $"{dir.Parent.FullName}\\__MT_GEN_{dir.Name}_{now}.txt";
                    using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Append, System.IO.FileAccess.Write))
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                    {
                        sw.WriteLine($"// ==== Generated with ShinyHobo's BG3 Modder's Multitool ====");
                        foreach (var file in files)
                        {
                            sw.WriteLine($"// === {file.Name} ===");
                            using (var input = new System.IO.StreamReader(file.FullName))
                            {
                                sw.WriteLine(input.ReadToEnd());
                            }
                        }
                    }
                    Directory.Delete(dir.FullName, true);
                }
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
            // Clean out temp folder
            var tempFolder = new DirectoryInfo(TempFolder);
            foreach (FileInfo file in tempFolder.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in tempFolder.GetDirectories())
            {
                dir.Delete(true);
            }

            // Pack mod
            var destination =  $"{TempFolder}\\{dirName}.pak";
            GeneralHelper.WriteToConsole(Resources.Destination, destination);
            GeneralHelper.WriteToConsole(Resources.AttemptingToPack);
            var buildDir = BuildPack(path);
            if(buildDir != null)
            {
                PackMod(buildDir, destination);
                Directory.Delete(buildDir, true);
                var modsPath = Path.Combine(path, "Mods");
                var pathList = Directory.GetDirectories(modsPath);
                if(pathList.Length == 0)
                {
                    var newModsPath = Path.Combine(modsPath, dirName);
                    Directory.CreateDirectory(newModsPath);
                    pathList = new string[] { newModsPath };
                }
                return GetMetalsxList(pathList);
            }
            return new List<string>();
        }
    }
}
