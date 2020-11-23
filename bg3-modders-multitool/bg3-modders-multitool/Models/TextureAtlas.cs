/// <summary>
/// The texture atlas model.
/// </summary>
namespace bg3_modders_multitool.Models
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
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
        public Bitmap Bitmap { get; set; }
        public BitmapImage BitmapImage { get; set; }
        public List<IconUV> Icons { get; set; }

        /// <summary>
        /// Gets an icon from the complete texture map.
        /// </summary>
        /// <param name="mapKey">The map key of the icon to match.</param>
        /// <returns>The bitmap of the icon.</returns>
        public BitmapImage GetIcon(string mapKey)
        {
            var icon = Icons.SingleOrDefault(i => i.MapKey == mapKey);
            if(icon != null)
            {
                var xPos = (int)(Width * icon.U1);
                var yPos = (int)(Height * icon.V1);
                Rectangle cloneRect = new Rectangle(xPos, yPos, IconWidth, IconHeight);
                var clone = BitmapImage.Clone();
                var bitmap = new Bitmap(clone.StreamSource);
                bitmap = bitmap.Clone(cloneRect, bitmap.PixelFormat);
                return ConvertBitmapToImage(bitmap);
            }
            return null;
        }

        /// <summary>
        /// Reads a texture atlas and the corresponding .dds file.
        /// </summary>
        /// <param name="path">The path to the texture atlas.</param>
        /// <param name="iconPath">The path to the .dds file.</param>
        /// <returns>A new texture atlas.</returns>
        public static TextureAtlas Read(string path, string iconPath)
        {
            var newTextureAtlas = new TextureAtlas { Path = path, Icons = new List<IconUV>() };
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            var textureAtlasInfo = doc.SelectSingleNode("//region[@id='TextureAtlasInfo']");
            textureAtlasInfo = textureAtlasInfo.SelectSingleNode("node[@id='root']");
            textureAtlasInfo = textureAtlasInfo.SelectSingleNode("children");
            var textureAtlasIconSize = textureAtlasInfo.SelectSingleNode("node[@id='TextureAtlasIconSize']");
            newTextureAtlas.IconHeight = int.Parse(textureAtlasIconSize.SelectSingleNode("attribute[@id='Height']").Attributes["value"].InnerText);
            newTextureAtlas.IconWidth = int.Parse(textureAtlasIconSize.SelectSingleNode("attribute[@id='Width']").Attributes["value"].InnerText);

            var textureAtlasPath = textureAtlasInfo.SelectSingleNode("node[@id='TextureAtlasPath']");
            newTextureAtlas.UUID = textureAtlasPath.SelectSingleNode("attribute[@id='UUID']").Attributes["value"].InnerText;

            var textureAtlasTextureSize = textureAtlasInfo.SelectSingleNode("node[@id='TextureAtlasTextureSize']");
            newTextureAtlas.Height = int.Parse(textureAtlasTextureSize.SelectSingleNode("attribute[@id='Height']").Attributes["value"].InnerText);
            newTextureAtlas.Width = int.Parse(textureAtlasTextureSize.SelectSingleNode("attribute[@id='Width']").Attributes["value"].InnerText);

            var iconUVList = doc.SelectSingleNode("//region[@id='IconUVList']");
            iconUVList = iconUVList.SelectSingleNode("node[@id='root']");
            iconUVList = iconUVList.SelectSingleNode("children");

            foreach(XmlElement iconNode in iconUVList.SelectNodes("node[@id='IconUV']"))
            {
                var icon = new IconUV {
                    MapKey = iconNode.SelectSingleNode("attribute[@id='MapKey']").Attributes["value"].InnerText,
                    U1 = float.Parse(iconNode.SelectSingleNode("attribute[@id='U1']").Attributes["value"].InnerText),
                    U2 = float.Parse(iconNode.SelectSingleNode("attribute[@id='U2']").Attributes["value"].InnerText),
                    V1 = float.Parse(iconNode.SelectSingleNode("attribute[@id='V1']").Attributes["value"].InnerText),
                    V2 = float.Parse(iconNode.SelectSingleNode("attribute[@id='V2']").Attributes["value"].InnerText)
                };
                newTextureAtlas.Icons.Add(icon);
            }

            using (var image = Pfim.Pfim.FromFile(iconPath))
            {
                var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                var bitmap = new Bitmap(image.Width, image.Height, image.Stride, PixelFormat.Format32bppArgb, data);
                newTextureAtlas.BitmapImage = ConvertBitmapToImage(bitmap);
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
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = ms;
            img.EndInit();
            bitmap.Dispose();
            return img;
        }
    }
}
