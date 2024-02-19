namespace bg3_modders_multitool.Services
{
    using bg3_modders_multitool.Models;
    using LSLib.LS;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Media.Imaging;
    using System.Xml;
    using System.Drawing;
    using System.Drawing.Imaging;
    using Alphaleonis.Win32.Filesystem;
    using System.Linq;


    /// <summary>
    /// Service for dealing with textures
    /// </summary>
    class TextureHelper
    {
        /// <summary>
        /// Reads a texture atlas and the corresponding .dds file.
        /// </summary>
        /// 
        /// <param name="contents">The path to the texture atlas.</param>
        /// <param name="pak">The pak.</param>
        /// <returns>A new texture atlas.</returns>
        public static TextureAtlas Read(byte[] contents, string path, string pak)
        {
            // TODO - memory optimize
            var newTextureAtlas = new TextureAtlas { Path = path, Icons = new List<IconUV>() };
            if(contents != null)
            {
                using (var contentStream = new System.IO.MemoryStream(contents))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(contentStream);
                    var textureAtlasInfo = doc.SelectSingleNode("//region[@id='TextureAtlasInfo']");
                    textureAtlasInfo = textureAtlasInfo.SelectSingleNode("node[@id='root']");
                    textureAtlasInfo = textureAtlasInfo.SelectSingleNode("children");

                    var textureAtlasPath = textureAtlasInfo.SelectSingleNode("node[@id='TextureAtlasPath']");
                    newTextureAtlas.UUID = textureAtlasPath.SelectSingleNode("attribute[@id='UUID']").Attributes["value"].InnerText;
                    newTextureAtlas.Path = $"Icons\\Public\\{pak}\\{textureAtlasPath.SelectSingleNode("attribute[@id='Path']").Attributes["value"].InnerText}".Replace("/", "\\");

                    var textureAtlasIconSize = textureAtlasInfo.SelectSingleNode("node[@id='TextureAtlasIconSize']");
                    newTextureAtlas.IconHeight = int.Parse(textureAtlasIconSize.SelectSingleNode("attribute[@id='Height']").Attributes["value"].InnerText, CultureInfo.InvariantCulture);
                    newTextureAtlas.IconWidth = int.Parse(textureAtlasIconSize.SelectSingleNode("attribute[@id='Width']").Attributes["value"].InnerText, CultureInfo.InvariantCulture);

                    var textureAtlasTextureSize = textureAtlasInfo.SelectSingleNode("node[@id='TextureAtlasTextureSize']");
                    newTextureAtlas.Height = int.Parse(textureAtlasTextureSize.SelectSingleNode("attribute[@id='Height']").Attributes["value"].InnerText, CultureInfo.InvariantCulture);
                    newTextureAtlas.Width = int.Parse(textureAtlasTextureSize.SelectSingleNode("attribute[@id='Width']").Attributes["value"].InnerText, CultureInfo.InvariantCulture);

                    var iconUVList = doc.SelectSingleNode("//region[@id='IconUVList']");
                    iconUVList = iconUVList.SelectSingleNode("node[@id='root']");
                    iconUVList = iconUVList.SelectSingleNode("children");

                    foreach (XmlElement iconNode in iconUVList.SelectNodes("node[@id='IconUV']"))
                    {
                        var icon = new IconUV
                        {
                            MapKey = iconNode.SelectSingleNode("attribute[@id='MapKey']").Attributes["value"].InnerText,
                            U1 = float.Parse(iconNode.SelectSingleNode("attribute[@id='U1']").Attributes["value"].InnerText, CultureInfo.InvariantCulture),
                            U2 = float.Parse(iconNode.SelectSingleNode("attribute[@id='U2']").Attributes["value"].InnerText, CultureInfo.InvariantCulture),
                            V1 = float.Parse(iconNode.SelectSingleNode("attribute[@id='V1']").Attributes["value"].InnerText, CultureInfo.InvariantCulture),
                            V2 = float.Parse(iconNode.SelectSingleNode("attribute[@id='V2']").Attributes["value"].InnerText, CultureInfo.InvariantCulture)
                        };
                        newTextureAtlas.Icons.Add(icon);
                    }
                }
            }

            return newTextureAtlas;
        }

        /// <summary>
        /// Converts a bitmap to a bitmap image source for use with xaml bindings.
        /// </summary>
        /// <param name="bitmap">The bitmap to convert.</param>
        /// <returns>The converted bitmap image.</returns>
        public static BitmapImage ConvertBitmapToImage(Bitmap bitmap)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = ms;
                img.EndInit();
                img.Freeze();
                bitmap.Dispose();
                return img;
            }
        }

        /// <summary>
        /// Converts a dds file into a bitmap image
        /// </summary>
        /// <param name="iconInfo">The dds file</param>
        /// <returns>The bitmap image</returns>
        public static BitmapImage ConvertDDSToBitmap(PackagedFileInfo iconInfo)
        {
            lock (iconInfo.PackageStream)
            {
                iconInfo.PackageStream.Position = 0;
                var iconStream = iconInfo.MakeStream();
                using (var image = Pfim.Pfimage.FromStream(iconStream))
                {
                    var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    var bitmap = new Bitmap(image.Width, image.Height, image.Stride, PixelFormat.Format32bppArgb, data);
                    iconInfo.ReleaseStream();
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);

                        using (var m2 = new System.IO.MemoryStream(ms.ToArray()))
                        {
                            var bmp = new Bitmap(ms);
                            return ConvertBitmapToImage(bmp);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts the dds content stream from individual GTP files
        /// </summary>
        /// <param name="path">The file path</param>
        /// <param name="writeToDisk">Whether or not to write the files to disk</param>
        /// <returns>The list of image arrays</returns>
        public static List<byte[]> ExtractGTPContents(string path, bool writeToDisk = false)
        {
            var helper = PakReaderHelper.GetPakHelper(path);
            var contents = helper.Helper.ReadPakFileContents(helper.Path, true);
            return ExtractGTPContents(helper.Path, helper.Helper, contents, writeToDisk);
        }
        /// <summary>
        /// Extracts the dds content stream from individual GTP files
        /// </summary>
        /// <param name="path">The file path</param>
        /// <param name="helper">The pak reader helper that contains the file</param>
        /// <param name="gtpContents">The GTP contents</param>
        /// <param name="writeToDisk">Whether or not to write the files to disk</param>
        /// <returns>The list of image arrays</returns>
        public static List<byte[]> ExtractGTPContents(string path, PakReaderHelper helper, byte[] gtpContents, bool writeToDisk = false)
        {
            var gtpFile = Path.GetFileNameWithoutExtension(path);
            var guid = gtpFile.Split('_').Last();
            var guidIndex = gtpFile.IndexOf(guid);
            var gtsFile = gtpFile.Substring(0, guidIndex - 1);
            gtsFile = path.Replace(gtpFile, gtsFile).Replace(".gtp", ".gts");
            var gtsContents = helper.ReadPakFileContents(gtsFile);

            var files = new List<byte[]>();
            if (gtsContents != null)
            {
                using (System.IO.MemoryStream gtsStream = new System.IO.MemoryStream(gtsContents))
                {
                    var vts = new LSLib.VirtualTextures.VirtualTileSet(gtsStream);
                    vts.SingleFileContents = gtpContents;
                    var vtsIndex = vts.FindPageFile(gtpFile);
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
                                    BinUtils.WriteStruct(binaryWriter, ref inStruct);
                                    binaryWriter.Write(tex.Data, 0, tex.Data.Length);
                                    if (writeToDisk)
                                    {
                                        var saveLoc = FileHelper.GetPath($"{helper.PakName}\\{path}") + $"_{layer}.dds";
                                        Directory.CreateDirectory(Path.GetDirectoryName(saveLoc));
                                        using (System.IO.FileStream file = new System.IO.FileStream(saveLoc, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                                        {
                                            output.WriteTo(file);
                                        }
                                    }
                                    else
                                    {
                                        files.Add(output.ToArray());
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    vts.ReleasePageFiles();
                }
            }

            return files;
        }
    }
}
