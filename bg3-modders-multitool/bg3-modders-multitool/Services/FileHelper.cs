/// <summary>
/// Service helper for dealing with files.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using Alphaleonis.Win32.Filesystem;
    using BrendanGrant.Helpers.FileAssociation;
    using LSLib.LS;
    using LSLib.LS.Enums;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public static class FileHelper
    {
        public static string[] ConvertableLsxResources = { ".lsf", ".lsb", ".lsbs", ".lsbc" };
        public static string[] MustRenameLsxResources = { ".lsbs", ".lsbc" };

        /// <summary>
        /// Converts the given file to .lsx type resource in-place
        /// </summary>
        /// <param name="file">The file to convert.</param>
        /// <param name="extension">The extension to convert to.</param>
        /// <param name="newPath">The new path to use.</param>
        /// <returns>The new file path.</returns>
        public static string Convert(string file, string extension, string newPath = null)
        {
            if(File.Exists(newPath)) {
                return newPath;
            }

            var originalExtension = Path.GetExtension(file);
            var newFile = file.Replace(originalExtension, $".{extension}");
            var isConvertableToLsx = newPath == null ? CanConvertToLsx(file) : CanConvertToLsx(newPath);
            var isConvertableToXml = originalExtension.Contains("loca");
            var isConvertableToLoca = originalExtension.Contains("xml");
            string path;
            if (string.IsNullOrEmpty(newPath))
            {
                path = GetPath(file);
                newPath = GetPath(newFile);
            }
            else
            {
                path = file;
            }
            if (!File.Exists(newPath) && isConvertableToLsx)
            {
                if(MustRenameLsxResources.Contains(originalExtension))
                {
                    var renamedPath = Path.ChangeExtension(path, originalExtension + ".lsf");
                    File.Move(path, renamedPath);
                    path = renamedPath;
                }
                var conversionParams = ResourceConversionParameters.FromGameVersion(Game.BaldursGate3);
                try
                {
                    Resource resource = ResourceUtils.LoadResource(path);
                    ResourceUtils.SaveResource(resource, newPath, conversionParams);
                }
                catch (Exception ex)
                {
                    // Larian decided to rename the .lsx to .lsbs instead of properly LSF encoding them
                    // These are invalid .lsbs/.lsbc files if this error pops up
                    if (ex.Message != "Invalid LSF signature; expected 464F534C, got 200A0D7B")
                    {
                        GeneralHelper.WriteToConsole($"Failed to convert resource to {extension} ({file}): {ex.Message}");
                    }
                }

                if (MustRenameLsxResources.Contains(originalExtension))
                {
                    File.Move(path, Path.ChangeExtension(path, ""));
                }
            }
            else if(!File.Exists(newPath) && isConvertableToXml)
            {
                using (var fs = File.Open(file, System.IO.FileMode.Open))
                {

                    try
                    {
                        var resource = LocaUtils.Load(fs, LocaFormat.Loca);
                        LocaUtils.Save(resource, newPath, LocaFormat.Xml);
                    } 
                    catch(Exception ex)
                    {
                        GeneralHelper.WriteToConsole($"Failed to convert resource to {extension} ({file}): {ex.Message}");
                    }
                }
            }
            else if(!File.Exists(newPath) && isConvertableToLoca)
            {
                using (var fs = File.Open(file, System.IO.FileMode.Open))
                {
                    try
                    {
                        var resource = LocaUtils.Load(fs, LocaFormat.Xml);
                        LocaUtils.Save(resource, newPath, LocaFormat.Loca);
                    }
                    catch (Exception ex)
                    {
                        GeneralHelper.WriteToConsole($"Failed to convert resource to {extension} ({file}): {ex.Message}");
                    }
                }
            }

            return isConvertableToLsx || isConvertableToXml || isConvertableToLoca ? newFile : file;
        }

        /// <summary>
        /// Checks to see if the file is convertable to lsx.
        /// </summary>
        /// <param name="file">The file to check.</param>
        /// <returns>Whether or not the file is convertable.</returns>
        public static bool CanConvertToLsx(string file)
        {
            if(string.IsNullOrEmpty(file))
            {
                return false;
            }
            var extension = Path.GetExtension(file);
            return ConvertableLsxResources.Contains(extension);
        }

        /// <summary>
        /// Converts a .wem file to an .ogg file in-place.
        /// </summary>
        /// <param name="file">The file to convert.</param>
        /// <returns>The new file path.</returns>
        public static string ConvertToOgg(string file)
        {
            try
            {
                if (CanConvertToOgg(file))
                {
                    file = GetPath(file);
                    var newFile = file.Replace(Path.GetExtension(file), ".ogg");
                    if (File.Exists(newFile))
                    {
                        return newFile;
                    }
                    var tempFile = Path.GetTempFileName();
                    using (System.IO.FileStream fs = new System.IO.FileStream(tempFile,
                        System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None,
                        4096, System.IO.FileOptions.RandomAccess))
                    {
                        WEMSharp.WEMFile wem = new WEMSharp.WEMFile(file, WEMSharp.WEMForcePacketFormat.NoForcePacketFormat);
                        var resource = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("bg3_modders_multitool.packed_codebooks_aoTuV_603.bin");
                        for (int i = 0; i < resource.Length; i++)
                            fs.WriteByte((byte)resource.ReadByte());
                        fs.Close();

                        wem.GenerateOGG(newFile, tempFile, false, false);
                        file = newFile;
                    }
                    System.IO.File.Delete(tempFile);
                }
            }
            catch
            {
                GeneralHelper.WriteToConsole($"Problem converting input file to .ogg!\n");
            }
            return file;
        }

        /// <summary>
        /// Checks to see if the file is convertable to ogg.
        /// </summary>
        /// <param name="file">The file to check.</param>
        /// <returns>Whether or not the file is convertable.</returns>
        public static bool CanConvertToOgg(string file)
        {
            var extension = Path.GetExtension(file);
            return extension == ".wem";
        }

        /// <summary>
        /// Converts .wem files to .ogg and plays them.
        /// </summary>
        /// <param name="file">The file to play.</param>
        public static void PlayAudio(string file)
        {
            file = ConvertToOgg(file);

            try
            {
                Task.Run(() => {
                    using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(file))
                    using (var waveOut = new NAudio.Wave.WaveOutEvent())
                    {
                        waveOut.Init(vorbisStream);
                        waveOut.Play(); // is async
                        while (waveOut.PlaybackState != NAudio.Wave.PlaybackState.Stopped);
                    }
                });
            }
            catch
            {
                GeneralHelper.WriteToConsole($"Problem playing audio file!!\n");
            }
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
        /// <returns>The full file path.</returns>
        public static string GetPath(string file)
        {
            if(!string.IsNullOrEmpty(file) && file.Contains($"{Directory.GetCurrentDirectory()}\\UnpackedData\\"))
                    return file;
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
                System.IO.TextWriter writer = null;
                try
                {
                    if (!Directory.Exists("Cache"))
                        Directory.CreateDirectory("Cache");
                    var contentsToWriteToFile = JsonConvert.SerializeObject(serialObject, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    writer = new System.IO.StreamWriter(file, false);
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
                System.IO.TextReader reader = null;
                try
                {
                    var stream = File.OpenText(file);
                    reader = stream;
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

        /// <summary>
        /// Creates and destroys the mod to skip splash screens.
        /// </summary>
        /// <param name="setting">Whether to enable or disable the mod.</param>
        public static void CreateDestroyQuickLaunchMod(bool setting)
        {
            var dataDir = Path.Combine(Directory.GetParent(Properties.Settings.Default.bg3Exe) + "\\", @"..\Data");
            var modLocation = Path.Combine(dataDir, "Video\\");
            var modFilepath = Path.Combine(modLocation,"Splash_Logo_Larian.bk2");
            if (setting)
            {
                GeneralHelper.WriteToConsole("Disabling splash screen...\n");
                if(!Directory.Exists(modLocation))
                    Directory.CreateDirectory(modLocation);
                if(!File.Exists(modFilepath))
                {
                    var modFile = File.Create(modFilepath);
                    modFile.Close();
                }
            }
            else
            {
                GeneralHelper.WriteToConsole("Enabling splash screen...\n");
                if(File.Exists(modFilepath))
                {
                    try
                    {
                        File.Delete(modFilepath);
                    }
                    catch
                    {
                        GeneralHelper.WriteToConsole("Failed to enable splash screen...\n");
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the file is valid xml
        /// </summary>
        /// <param name="filepath">The file path to test</param>
        /// <returns>Whether the file is valid or not</returns>
        public static bool TryParseXml(string filepath)
        {
            try
            {
                System.Xml.Linq.XDocument.Load(filepath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
