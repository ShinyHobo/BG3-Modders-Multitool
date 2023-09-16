namespace bg3_modders_multitool.Services
{
    using LSLib.LS;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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
    }
}
