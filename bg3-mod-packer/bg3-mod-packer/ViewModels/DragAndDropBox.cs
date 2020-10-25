using System.IO;
using System.Windows;
using System.Xml;
using System.Collections.Generic;
using Newtonsoft.Json;

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

                            var mods = new List<Models.Meta>();

                            foreach(string meta in metaList)
                            {
                                // generate .pak files


                                // generate info.json section
                                XmlDocument doc = new XmlDocument();
                                doc.Load(meta);

                                var metadata = new Models.Meta
                                {
                                    Author = doc.SelectSingleNode("//attribute[@id='Author']").Attributes["value"].InnerText,
                                    Name = doc.SelectSingleNode("//attribute[@id='Name']").Attributes["value"].InnerText,
                                    Description = doc.SelectSingleNode("//attribute[@id='Description']").Attributes["value"].InnerText,
                                    Version = doc.SelectSingleNode("//attribute[@id='Version']").Attributes["value"].InnerText,
                                    Folder = doc.SelectSingleNode("//attribute[@id='Folder']").Attributes["value"].InnerText,
                                    UUID = doc.SelectSingleNode("//attribute[@id='UUID']").Attributes["value"].InnerText,
                                    MD5 = ""
                                };

                                mods.Add(metadata);
                            }

                            var json = $"{{\"Mods\":{JsonConvert.SerializeObject(mods)} }}";
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
