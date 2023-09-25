namespace bg3_modders_multitool.Services
{
    using System.Collections.Generic;

    /// <summary>
    /// Service for dealing with textures
    /// </summary>
    class TextureHelper
    {
        /// <summary>
        /// Extracts the dds content stream from individual GTP files
        /// </summary>
        /// <param name="name">The name of the page file</param>
        /// <param name="contents">The content array</param>
        /// <returns></returns>
        public static List<byte[]> ExtractGTPContents(string name, byte[] contents)
        {
            var files = new List<byte[]>();
            using (System.IO.MemoryStream gtsStream = new System.IO.MemoryStream(contents))
            {
                var vts = new LSLib.VirtualTextures.VirtualTileSet(gtsStream);
                vts.SingleFileContents = contents;
                var vtsIndex = vts.FindPageFile(name);
                var fileInfo = vts.PageFileInfos[vtsIndex];
                try
                {
                    for (var layer = 0; layer < vts.TileSetLevels.Length; layer++)
                    {
                        LSLib.VirtualTextures.BC5Image tex = null;
                        var level = 0;
                        try
                        {
                            do
                            {
                                tex = vts.ExtractPageFileTexture(vtsIndex, level, layer);
                                level++;
                            } while (tex == null && level < vts.TileSetLevels.Length);
                        }
                        catch { }
                        if (tex != null)
                        {
                            LSLib.VirtualTextures.DDSHeader inStruct = default;
                            inStruct.dwMagic = 542327876u;
                            inStruct.dwSize = 124u;
                            inStruct.dwFlags = 4103u;
                            inStruct.dwWidth = (uint)tex.Width;
                            inStruct.dwHeight = (uint)tex.Height;
                            inStruct.dwPitchOrLinearSize = (uint)(tex.Width * tex.Height);
                            inStruct.dwDepth = 1u;
                            inStruct.dwMipMapCount = 1u;
                            inStruct.dwPFSize = 32u;
                            inStruct.dwPFFlags = 4u;
                            inStruct.dwFourCC = 894720068u;
                            inStruct.dwCaps = 4096u;
                            using (System.IO.MemoryStream output = new System.IO.MemoryStream())
                            using (System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(output))
                            {
                                LSLib.LS.BinUtils.WriteStruct(binaryWriter, ref inStruct);
                                binaryWriter.Write(tex.Data, 0, tex.Data.Length);
                                files.Add(output.ToArray());
                            }
                        }
                    }
                }
                catch { }

                vts.ReleasePageFiles();
            }
            return files;
        }
    }
}
