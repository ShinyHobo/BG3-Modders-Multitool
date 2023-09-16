namespace bg3_modders_multitool.Services
{
    using LSLib.LS;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

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

        public string ReadPakFileContents(string filePath)
        {
            var file = PackagedFiles.First(pf => pf.Name == filePath);
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

            return System.Text.Encoding.UTF8.GetString(output);
        }
    }
}
