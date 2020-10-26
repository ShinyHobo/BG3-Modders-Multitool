using System.IO;
using System.Windows;
using System.Xml;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System;
using System.IO.Compression;
using bg3_mod_packer.Models;

namespace bg3_mod_packer.ViewModels
{
    public class FolderDragDropHelper
    {
        /// <summary>
        /// Process the file/folder drop.
        /// </summary>
        /// <param name="data">Drop data</param>
        /// <returns>Success</returns>
        public static bool ProcessDrop(IDataObject data)
        {
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                var fileDrop = data.GetData(DataFormats.FileDrop, true);
                if (fileDrop is string[] filesOrDirectories && filesOrDirectories.Length > 0)
                {
                    foreach (string fullPath in filesOrDirectories)
                    {
                        // Only accept directory
                        if (Directory.Exists(fullPath))
                        {
                            var metaList = new List<string>();
                            foreach (string mod in Directory.GetDirectories(fullPath + "/Mods"))
                            {
                                foreach (string file in Directory.GetFiles(mod))
                                {
                                    if(Path.GetFileName(file).Equals("meta.lsx"))
                                    {
                                        metaList.Add(file);
                                    }
                                }
                            }

                            if(metaList.Count == 0)
                            {
                                // meta.lsx not found, discontinue
                                return false;
                            }

                            var mods = new List<MetaLsx>();
                            var tempFolder = Path.GetTempPath() + "BG3ModPacker";
                            Directory.CreateDirectory(tempFolder);

                            var dirName = new DirectoryInfo(fullPath).Name;
                            var destination = tempFolder + $"\\{dirName}.pak";
                            var pathToDivine = Properties.Settings.Default.divineExe;
                            var divine = @"/c " + pathToDivine + $" -g \"bg3\" --action \"create-package\" --source \"{fullPath}\" --destination \"{destination}\" -l \"all\"";

                            // generate .pak files
                            var process = new System.Diagnostics.Process();
                            var startInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = divine,
                                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                                UseShellExecute = false,
                                RedirectStandardOutput = true
                            };
                            process.StartInfo = startInfo;
                            process.Start();
                            var stdout = process.StandardOutput.ReadToEnd();
                            process.WaitForExit();

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
                                    UUID = doc.SelectSingleNode("//attribute[@id='UUID']").Attributes["value"].InnerText
                                };

                                mods.Add(metadata);
                            }

                            info.Mods = mods;
                            var json = JsonConvert.SerializeObject(info);
                            File.WriteAllText(tempFolder + @"\info.json", json);

                            // save zip next to folder that was dropped
                            var parentDir = Directory.GetParent(fullPath);
                            var zip = $"{parentDir.ToString()}\\{mods[0].Name}.zip";
                            if(File.Exists(zip))
                            {
                                File.Delete(zip);
                            }
                            ZipFile.CreateFromDirectory(tempFolder, zip);

                            // cleanup temp folder
                            DirectoryInfo di = new DirectoryInfo(tempFolder);

                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }

                            return true;
                        }
                        else
                        {
                            // File dropping unsupported
                        }
                    }
                }
            }
            return false;
        }
    }
}
