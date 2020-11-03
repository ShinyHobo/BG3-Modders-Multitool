/// <summary>
/// The view model for use with the drag and drop box.
/// </summary>
namespace bg3_mod_packer.ViewModels
{
    using System.IO;
    using System.Windows;
    using System;
    using bg3_mod_packer.Helpers;

    public class DragAndDropBox
    {
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
                        foreach (string fullPath in filesOrDirectories)
                        {
                            // Only accept directory
                            if (Directory.Exists(fullPath))
                            {
                                var dirName = new DirectoryInfo(fullPath).Name;
                                ((Models.MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"Directory name: {dirName}\n";
                                var destination = DragAndDropHelper.TempFolder + $"\\{dirName}.pak";
                                ((Models.MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"Destination: {destination}\n";
                                ((Models.MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"Attempting to pack mod.\n";
                                DragAndDropHelper.PackMod(fullPath, destination);
                                var metaList = DragAndDropHelper.GetMetalsxList(Directory.GetDirectories(fullPath + "\\Mods"));
                                DragAndDropHelper.GenerateInfoJson(destination, metaList);
                                DragAndDropHelper.GenerateZip(fullPath, dirName);
                                DragAndDropHelper.CleanTempDirectory();
                            }
                            else
                            {
                                // File dropping unsupported
                                ((Models.MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"File dropping is not yet supported.";
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ((Models.MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += ex.Message;
            }
        }
    }
}
