using System;
using System.IO;
using System.Windows;

namespace bg3_mod_packer.ViewModels
{
    public class FolderDragDropHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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
                            var metadata = fullPath + "/Mods/meta.lsx";

                            if (!File.Exists(metadata))
                            {
                                // meta.lsx not found, discontinue
                                return false;
                            }


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
