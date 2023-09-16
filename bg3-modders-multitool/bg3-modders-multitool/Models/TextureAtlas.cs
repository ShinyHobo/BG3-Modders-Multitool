/// <summary>
/// The texture atlas model.
/// </summary>
namespace bg3_modders_multitool.Models
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Services;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Media.Imaging;
    using System.Xml;

    public class TextureAtlas
    {
        public string Path { get; set; }
        public string UUID { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int IconHeight { get; set; }
        public int IconWidth { get; set; }
        public byte[] AtlasImage { get; set; }
        public List<IconUV> Icons { get; set; }

        /// <summary>
        /// Gets an icon from the complete texture map.
        /// </summary>
        /// <param name="mapKey">The map key of the icon to match.</param>
        /// <returns>The bitmap of the icon.</returns>
        public BitmapImage GetIcon(string mapKey)
        {
            var icon = Icons.SingleOrDefault(i => i.MapKey == mapKey);
            if(icon != null && AtlasImage != null)
            {
                var xPos = (int)(Width * icon.U1);
                var yPos = (int)(Height * icon.V1);
                Rectangle cloneRect = new Rectangle(xPos, yPos, IconWidth, IconHeight);
                using (var ms = new System.IO.MemoryStream(AtlasImage))
                {
                    var bmp = new Bitmap(ms);
                    bmp = bmp.Clone(cloneRect, bmp.PixelFormat);
                    return ConvertBitmapToImage(bmp);
                }
            }
            return null;
        }

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
            using(var contentStream = new System.IO.MemoryStream(contents))
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

            return newTextureAtlas;
        }

        /// <summary>
        /// Converts a bitmap to a bitmap image source for use with xaml bindings.
        /// </summary>
        /// <param name="bitmap">The bitmap to convert.</param>
        /// <returns>The converted bitmap image.</returns>
        private static BitmapImage ConvertBitmapToImage(Bitmap bitmap)
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
        /// <param name="file">The dds file</param>
        /// <returns>The bitmap image</returns>
        public static BitmapImage ConvertDDSToBitmap(string file)
        {
            using (var image = Pfim.Pfimage.FromFile(file))
            {
                var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                var bitmap = new Bitmap(image.Width, image.Height, image.Stride, PixelFormat.Format32bppArgb, data);
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
}
