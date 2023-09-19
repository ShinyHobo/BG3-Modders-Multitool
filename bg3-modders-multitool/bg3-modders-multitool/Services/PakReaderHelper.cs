namespace bg3_modders_multitool.Services
{
    using LSLib.LS;
    using LSLib.LS.Enums;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper for reading files directly from pak files without unpacking
    /// </summary>
    public class PakReaderHelper
    {
        private PackageReader PackageReader;
        private Package Package;
        public string PakName { get; private set; }
        public List<PackagedFileInfo> PackagedFiles { get; private set; }

        public PakReaderHelper(string pakPath) {
            PackageReader = new PackageReader(pakPath);
            PakName = Path.GetFileNameWithoutExtension(pakPath);
            try
            {
                Package = PackageReader.Read();
                PackagedFiles = Package.Files.Select(f => f as PackagedFileInfo).ToList();
            }
            catch(NotAPackageException) { }
        }

        /// <summary>
        /// Reads the file from this pak and gets the contents
        /// </summary>
        /// <param name="filePath">The internal pak file path</param>
        /// <returns>The file contents</returns>
        public byte[] ReadPakFileContents(string filePath)
        {
            var file = PackagedFiles.FirstOrDefault(pf => pf.Name == filePath.Replace('\\', '/'));
            if (file == null)
                return null;

            byte[] output;
            byte[] buffer = new byte[32768];
            try
            {
                lock (file.PackageStream)
                {
                    file.PackageStream.Position = 0;
                    using (Stream ms = file.MakeStream())
                    using (BinaryReader reader = new BinaryReader(ms))
                    using (MemoryStream msStream = new MemoryStream())
                    {
                        int count;
                        while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                            msStream.Write(buffer, 0, count);
                        output = msStream.ToArray();
                    }
                }
            }
            finally
            {
                file.ReleaseStream();
            }

            return output;
        }

        /// <summary>
        /// Decompresses the selected file directly from the pak into its readable format
        /// </summary>
        /// <param name="filePath">The pak file path</param>
        public string DecompressPakFile(string filePath)
        {
            var file = PackagedFiles.FirstOrDefault(pf => pf.Name == filePath.Replace('\\', '/'));
            if (file != null)
            {
                lock(file.PackageStream)
                {
                    file.PackageStream.Position = 0;
                    var originalExtension = Path.GetExtension(filePath);
                    var isConvertableToLsx = FileHelper.CanConvertToLsx(filePath);
                    var isConvertableToXml = originalExtension.Contains("loca");
                    var conversionParams = ResourceConversionParameters.FromGameVersion(Game.BaldursGate3);
                    if (isConvertableToLsx)
                    {
                        var newFile = filePath.Replace(originalExtension, $"{originalExtension}.lsx");
                        var format = ResourceUtils.ExtensionToResourceFormat(filePath);
                        var resource = ResourceUtils.LoadResource(file.MakeStream(), format, ResourceLoadParameters.FromGameVersion(Game.BaldursGate3));
                        ResourceUtils.SaveResource(resource, FileHelper.GetPath($"{PakName}\\{newFile}"), conversionParams);
                        return FileHelper.GetPath($"{PakName}\\{newFile}");
                    }
                    else if (isConvertableToXml)
                    {
                        var newFile = filePath.Replace(originalExtension, $"{originalExtension}.xml");
                        var resource = LocaUtils.Load(file.MakeStream(), LocaFormat.Loca);
                        LocaUtils.Save(resource, FileHelper.GetPath($"{PakName}\\{newFile}"), LocaFormat.Xml);
                        return FileHelper.GetPath($"{PakName}\\{newFile}");
                    }
                    else
                    {
                        var path = FileHelper.GetPath($"{PakName}\\{filePath}");
                        FileManager.TryToCreateDirectory(path);
                        var contents = ReadPakFileContents(filePath);
                        using (FileStream fileStream = Alphaleonis.Win32.Filesystem.File.Open(path, FileMode.Create, FileAccess.Write))
                        {
                            fileStream.Write(contents, 0, contents.Length);
                        }
                        return path;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the list of pak directory infomation
        /// </summary>
        /// <returns>The pak list</returns>
        public static List<string> GetPakList()
        {
            if(string.IsNullOrEmpty(Properties.Settings.Default.bg3Exe))
            {
                GeneralHelper.WriteToConsole(Properties.Resources.InvalidBg3Location);
                return new List<string>();
            }
            return Alphaleonis.Win32.Filesystem.Directory.GetFiles(FileHelper.DataDirectory, "*.pak", SearchOption.AllDirectories).Select(file => Path.GetFullPath(file)).ToList();
        }

        /// <summary>
        /// Opens and decompresses the selected pak file if it is not already
        /// </summary>
        /// <param name="selectedPath">The selected pak file path</param>
        public static string OpenPakFile(string selectedPath)
        {
            if(selectedPath != null)
            {
                selectedPath = selectedPath.Replace(FileHelper.UnpackedDataPath + "\\", string.Empty);
                if (!File.Exists(FileHelper.GetPath(selectedPath)))
                {
                    var pak = selectedPath.Split('\\')[0];
                    var pakFile = $"{pak}.pak";
                    var paks = GetPakList();
                    var pakPath = paks.FirstOrDefault(p => p.EndsWith("\\" + pakFile));
                    if (File.Exists(pakPath))
                    {
                        var helper = new PakReaderHelper(pakPath);
                        return helper.DecompressPakFile(GetPakPath(selectedPath));
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the internal pak file path
        /// </summary>
        /// <param name="path">The path to convert</param>
        /// <returns>The internal path</returns>
        public static string GetPakPath(string path)
        {
            var pakName = path.Split('\\')[0];
            var regex = new Regex(Regex.Escape(pakName + "\\"));
            return regex.Replace(path, string.Empty, 1);
        }

        /// <summary>
        /// Gets the list of all pak reader helpers
        /// </summary>
        /// <returns>The list of pak reader helpers</returns>
        public static List<PakReaderHelper> GetPakHelpers()
        {
            var pakHelpers = new List<PakReaderHelper>();
            var paks = GetPakList();
            foreach (var pak in paks)
            {
                var helper = new PakReaderHelper(pak);
                if (helper.PackagedFiles != null)
                {
                    pakHelpers.Add(helper);
                }
            }
            return pakHelpers;
        }
    }
}
