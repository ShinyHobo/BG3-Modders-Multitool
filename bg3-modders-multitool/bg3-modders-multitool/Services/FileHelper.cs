/// <summary>
/// Service helper for dealing with files.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using bg3_modders_multitool.ViewModels;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;

    public static class FileHelper
    {
        public static string[] ConvertableLsxResources = { ".lsf", ".lsb" };

        /// <summary>
        /// Converts the given file to .lsx in-place
        /// </summary>
        /// <param name="file">The file to convert.</param>
        /// <returns>The new file path.</returns>
        public static string Convert(string file, string extension, string newPath = null)
        {
            var path = string.Empty;
            var originalExtension = Path.GetExtension(file);
            var newFile = file.Replace(originalExtension, $".{extension}");
            var isConvertable = true;
            if (string.IsNullOrEmpty(newPath))
            {
                path = GetPath(file);
                newPath = GetPath(newFile);
                isConvertable = CanConvertToLsx(file);
            }
            else
            {
                path = file;
            }
            if (!File.Exists(newPath) && isConvertable)
            {
                var divine = $" -g \"bg3\" --action \"convert-resource\" --output-format \"{extension}\" --source \"{path}\" --destination \"{newPath}\" -l \"all\"";
                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    FileName = Properties.Settings.Default.divineExe,
                    Arguments = divine,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if(string.IsNullOrEmpty(newPath))
                    {
                        ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += process.StandardOutput.ReadToEnd();
                        ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += process.StandardError.ReadToEnd();
                    }
                });
            }

            return isConvertable ? newFile : file;
        }

        public static bool CanConvertToLsx(string file)
        {
            var extension = Path.GetExtension(file);
            return ConvertableLsxResources.Contains(extension);
        }

        /// <summary>
        /// Gets a list of files in a directory.
        /// </summary>
        /// <param name="directory">The directory root to search.</param>
        /// <returns>A list of files in the directory.</returns>
        public static List<string> DirectorySearch(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var fileList = RecursiveFileSearch(directory);
            if (fileList.Count == 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"No files found!\n";
                });
            }
            return fileList;
        }

        /// <summary>
        /// Recursively searches for all files within the given directory.
        /// </summary>
        /// <param name="directory">The directory root to search.</param>
        /// <returns>A list of files in the directory.</returns>
        private static List<string> RecursiveFileSearch(string directory)
        {
            var fileList = new List<string>();
            foreach (string dir in Directory.GetDirectories(directory))
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    fileList.Add(file);
                }
                fileList.AddRange(RecursiveFileSearch(dir));
            }
            return fileList;
        }

        /// <summary>
        /// Gets the complete list of extensions of the files in the given file list.
        /// </summary>
        /// <param name="fileList">The file list to scan.</param>
        /// <returns>The list of file extensions.</returns>
        public static List<string> GetFileExtensions(List<string> fileList)
        {
            var extensions = new List<string>();
            foreach (var file in fileList)
            {
                var extension = Path.GetExtension(file);
                if (!extensions.Contains(extension))
                {
                    extensions.Add(extension);
                }
            }
            return extensions;
        }

        /// <summary>
        /// Gets a standard path for files.
        /// </summary>
        /// <param name="file">The file to generate a path for.</param>
        /// <returns></returns>
        public static string GetPath(string file)
        {
            return $"{Directory.GetCurrentDirectory()}\\UnpackedData\\{file}";
        }

        /// <summary>
        /// Opens the given file path in the default program.
        /// </summary>
        /// <param name="file">The file to open.</param>
        public static void OpenFile(string file)
        {
            var path = GetPath(file);
            if (File.Exists(@"\\?\" + path))
            {
                Process.Start(path);
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"File does not exist on the given path ({path}).\n";
            }
        }
    }
}
