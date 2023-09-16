namespace bg3_modders_multitool.Services
{
    using LSLib.LS;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class PakReaderHelper
    {
        private PackageReader PackageReader;
        private Package Package;
        public string PakName { get; private set; }
        public List<AbstractFileInfo> PackagedFiles { get; private set; }

        public PakReaderHelper(string pakPath) {
            PackageReader = new PackageReader(pakPath);
            PakName = Path.GetFileNameWithoutExtension(pakPath);
            try
            {
                Package = PackageReader.Read();
                PackagedFiles = Package.Files;
            }
            catch(NotAPackageException) { }
        }

        public string ReadPakFileContents(string filePath)
        {
            var contents = string.Empty;
            var file = PackagedFiles.First(pf => pf.Name == filePath);
            byte[] array = new byte[32768];
            try
            {
                using (Stream stream = file.MakeStream())
                using (BinaryReader binaryReader = new BinaryReader(stream))
                {
                    int count;
                    while ((count = binaryReader.Read(array, 0, array.Length)) > 0)
                    {
                        contents += System.Text.Encoding.UTF8.GetString(array);
                    }
                }
            }
            finally
            {
                file.ReleaseStream();
            }

            return contents;
        }
    }
}
