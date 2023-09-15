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
        private List<AbstractFileInfo> PackagedFiles;

        public PakReaderHelper(string pakPath) {
            // "H:\\SteamLibrary\\steamapps\\common\\Baldurs Gate 3\\Data\\Gustav.pak"

            PackageReader = new PackageReader(pakPath);
            Package = PackageReader.Read();
            PackagedFiles = Package.Files;
        }

        public string ReadPakFileContents(string filePath)
        {
            var file = PackagedFiles.First(pf => pf.Name == filePath);
            byte[] array = new byte[32768];
            var contents = string.Empty;
            try
            {
                using(Stream stream = file.MakeStream())
                {
                    using (BinaryReader binaryReader = new BinaryReader(stream))
                    {
                        int count;
                        while ((count = binaryReader.Read(array, 0, array.Length)) > 0)
                        {
                            contents += System.Text.Encoding.UTF8.GetString(array);
                        }
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
