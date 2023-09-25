/// <summary>
/// The texture atlas model.
/// </summary>
namespace bg3_modders_multitool.Models
{
    using bg3_modders_multitool.Services;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Media.Imaging;

    public class TextureAtlas
    {
        public string Path { get; set; }
        public string UUID { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int IconHeight { get; set; }
        public int IconWidth { get; set; }
        public List<IconUV> Icons { get; set; }

        /// <summary>
        /// Gets an icon from the complete texture map.
        /// </summary>
        /// <param name="mapKey">The map key of the icon to match.</param>
        /// <returns>The bitmap of the icon.</returns>
        public BitmapImage GetIcon(string mapKey, PakReaderHelper pak)
        {
            var icon = Icons.SingleOrDefault(i => i.MapKey == mapKey);

            if (icon != null)
            {
                var contents = pak.ReadPakFileContents(PakReaderHelper.GetPakPath(Path));
                using (var contentStream = new System.IO.MemoryStream(contents))
                using (var image = Pfim.Pfimage.FromStream(contentStream))
                {
                    var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    var original = new Bitmap(image.Width, image.Height, image.Stride, PixelFormat.Format32bppArgb, data);
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        original.Save(ms, ImageFormat.Png);
                        original.Dispose();

                        var xPos = (int)(Width * icon.U1);
                        var yPos = (int)(Height * icon.V1);
                        Rectangle cloneRect = new Rectangle(xPos, yPos, IconWidth, IconHeight);

                        using (var newStream = new System.IO.MemoryStream(ms.ToArray()))
                        {
                            var bmp = new Bitmap(newStream);
                            bmp = bmp.Clone(cloneRect, bmp.PixelFormat);
                            return TextureHelper.ConvertBitmapToImage(bmp);
                        }
                    }
                }
            }
            return null;
        }
    }
}
