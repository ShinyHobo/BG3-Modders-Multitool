namespace bg3_modders_multitool.Services
{
    using LSLib.LS;
    using LSLib.LS.Enums;
    using System;
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
        public byte[] ReadPakFileContents(string filePath, bool convert = false)
        {
            var file = PackagedFiles.FirstOrDefault(pf => pf.Name == filePath.Replace('\\', '/'));
            if (file == null)
                return null;

            byte[] buffer = new byte[32768];
            try
            {
                using (MemoryStream originalStream = new MemoryStream())
                {
                    lock (file.PackageStream)
                    {
                        file.PackageStream.Position = (long)file.OffsetInFile;
                        using (Stream ms = file.MakeStream())
                        using (BinaryReader reader = new BinaryReader(ms))
                        {
                            int count;
                            while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                                originalStream.Write(buffer, 0, count);
                        }
                    }
                    
                    if(file.SizeOnDisk != 0)
                    {
                        var ext = Path.GetExtension(file.Name);
                        var canConvertToLsx = FileHelper.ConvertableLsxResources.Contains(ext);
                        if(convert && canConvertToLsx)
                        {
                            using (MemoryStream newOutStream = new MemoryStream())
                            {
                                try
                                {
                                    var conversionParams = ResourceConversionParameters.FromGameVersion(Game.BaldursGate3);
                                    var format = ResourceUtils.ExtensionToResourceFormat(filePath);
                                    originalStream.Position = 0;
                                    var resource = ResourceUtils.LoadResource(originalStream, format, ResourceLoadParameters.FromGameVersion(Game.BaldursGate3));
                                    LSXWriter lSXWriter = new LSXWriter(newOutStream);
                                    lSXWriter.Version = conversionParams.LSX;
                                    lSXWriter.PrettyPrint = conversionParams.PrettyPrint;
                                    conversionParams.ToSerializationSettings(lSXWriter.SerializationSettings);
                                    lSXWriter.Write(resource);
                                    return newOutStream.ToArray();
                                }
                                catch (Exception ex)
                                {
                                    if (!FileHelper.IsSpecialLSFSignature(ex.Message))
                                    {
                                        GeneralHelper.WriteToConsole(Properties.Resources.FailedToConvertResource, ".lsf", file.Name, ex.Message.Replace(Directory.GetCurrentDirectory(), string.Empty));
                                    }
                                }
                            }
                        }
                        else if(convert && ext == ".loca")
                        {
                            using (MemoryStream newOutStream = new MemoryStream())
                            {
                                try
                                {
                                    var conversionParams = ResourceConversionParameters.FromGameVersion(Game.BaldursGate3);
                                    var format = LocaUtils.ExtensionToFileFormat(filePath);
                                    originalStream.Position = 0;
                                    var resource = LocaUtils.Load(originalStream, format);
                                    new LocaXmlWriter(newOutStream).Write(resource);
                                    return newOutStream.ToArray();
                                }
                                catch (Exception ex)
                                {
                                    GeneralHelper.WriteToConsole(Properties.Resources.FailedToConvertResource, ".xml", file.Name, ex.Message.Replace(Directory.GetCurrentDirectory(), string.Empty));
                                }
                            }
                        }
                        else
                        {
                            return originalStream.ToArray();
                        }
                    }
                }
            }
            finally
            {
                file.ReleaseStream();
            }

            return new byte[0];
        }

        /// <summary>
        /// Decompresses the selected file directly from the pak into its readable format
        /// </summary>
        /// <param name="filePath">The pak file path</param>
        /// <param name="altPath">Alternate path to save the file to</param>
        public string DecompressPakFile(string filePath, string altPath = null)
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
                    try
                    {
                        if (isConvertableToLsx && file.SizeOnDisk != 0)
                        {
                            var format = ResourceUtils.ExtensionToResourceFormat(filePath);
                            var resource = ResourceUtils.LoadResource(file.MakeStream(), format, ResourceLoadParameters.FromGameVersion(Game.BaldursGate3));
                            var newFile = filePath.Replace(originalExtension, $"{originalExtension}.lsx");
                            newFile = string.IsNullOrEmpty(altPath) ? FileHelper.GetPath($"{PakName}\\{newFile}") : $"{altPath}\\{newFile}";
                            ResourceUtils.SaveResource(resource, newFile, conversionParams);
                            return newFile;
                        }
                        else if (isConvertableToXml && file.SizeOnDisk != 0)
                        {
                            var resource = LocaUtils.Load(file.MakeStream(), LocaFormat.Loca);
                            var newFile = filePath.Replace(originalExtension, $"{originalExtension}.xml");
                            newFile = string.IsNullOrEmpty(altPath) ? FileHelper.GetPath($"{PakName}\\{newFile}") : $"{altPath}\\{newFile}";
                            LocaUtils.Save(resource, newFile, LocaFormat.Xml);
                            return newFile;
                        }
                    }
                    catch(Exception ex)
                    {
                        if (FileHelper.IsSpecialLSFSignature(ex.Message))
                        {
                            GeneralHelper.WriteToConsole(Properties.Resources.FailedToConvertResource, isConvertableToLsx ? ".lsx" : ".xml", file.Name, ex.Message.Replace(Directory.GetCurrentDirectory(), string.Empty));
                        }
                    }

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
