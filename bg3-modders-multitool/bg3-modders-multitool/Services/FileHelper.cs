/// <summary>
/// Service helper for dealing with files.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using BrendanGrant.Helpers.FileAssociation;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    public static class FileHelper
    {
        public static string[] ConvertableLsxResources = { ".lsf", ".lsb", ".lsbs", ".lsbc" };
        public static string[] MustRenameLsxResources = { ".lsbs", ".lsbc" };

        /// <summary>
        /// Converts the given file to .lsx in-place
        /// </summary>
        /// <param name="file">The file to convert.</param>
        /// <returns>The new file path.</returns>
        public static string Convert(string file, string extension, string newPath = null)
        {
            var originalExtension = Path.GetExtension(file);
            var newFile = file.Replace(originalExtension, $".{extension}");
            var isConvertable = true;
            string path;
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
                if(MustRenameLsxResources.Contains(originalExtension))
                {
                    var renamedPath = Path.ChangeExtension(path, originalExtension + ".lsf");
                    File.Move(path, renamedPath);
                    path = renamedPath;
                }
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
                if(string.IsNullOrEmpty(newPath))
                {
                    GeneralHelper.WriteToConsole(process.StandardOutput.ReadToEnd());
                    GeneralHelper.WriteToConsole(process.StandardError.ReadToEnd());
                }
                if (MustRenameLsxResources.Contains(originalExtension))
                {
                    File.Move(path, Path.ChangeExtension(path, ""));
                }
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
                GeneralHelper.WriteToConsole($"No files found!\n");
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
                try
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        fileList.Add(file);
                    }
                    fileList.AddRange(RecursiveFileSearch(dir));
                }
                catch
                {
                    GeneralHelper.WriteToConsole($"Could not read from directory: {directory}\n");
                }
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
        /// Determines if the file path is a .GR2 mesh.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>Whether or not the file is a .GR2 file.</returns>
        public static bool IsGR2(string path)
        {
            return Path.GetExtension(path).Contains(".GR2") || Path.GetExtension(path).Contains(".gr2");
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
                if(IsGR2(path))
                {
                    var dae = Path.ChangeExtension(path,".dae");
                    // determine if you can determine if there is a default program
                    var fileAssociation = new FileAssociationInfo(".dae");
                    if(fileAssociation.Exists)
                        Process.Start(dae);
                    // open folder
                    Process.Start("explorer.exe", $"/select,{dae}");
                }
                else
                {
                    Process.Start(path);
                }
            }
            else
            {
                GeneralHelper.WriteToConsole($"File does not exist on the given path ({path}).\n");
            }
        }

        /// <summary>
        /// Serializes an object and saves it as a json file.
        /// </summary>
        /// <param name="serialObject">The object to serialize</param>
        /// <param name="filename">The filename to use, without extension</param>
        public static void SerializeObject(object serialObject, string filename)
        {
            var file = $"Cache/{filename}.json";
            if (!File.Exists(file))
            {
                GeneralHelper.WriteToConsole($"Caching {filename}...\n");
                TextWriter writer = null;
                try
                {
                    var contentsToWriteToFile = JsonConvert.SerializeObject(serialObject, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    writer = new StreamWriter(file, false);
                    writer.Write(contentsToWriteToFile);
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                }
            }
        }

        /// <summary>
        /// Deserializes an object from a saved json file.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="filename">The filename, without extension.</param>
        /// <returns>The deserialized object, or null if not found.</returns>
        public static T DeserializeObject<T>(string filename)
        {
            var file = $"Cache/{filename}.json";
            T objectOut = default(T);
            if (File.Exists(file))
            {
                GeneralHelper.WriteToConsole($"Loading {filename} from cache...\n");
                TextReader reader = null;
                try
                {
                    reader = new StreamReader(file);
                    var fileContents = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(fileContents);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            return objectOut;
        }
    }
}
