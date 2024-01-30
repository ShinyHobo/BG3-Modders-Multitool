/// <summary>
/// Converts formatted text binding to XAML
/// https://stackoverflow.com/questions/34095182/xaml-convert-textblock-text-to-inlines
/// </summary>
namespace bg3_modders_multitool.XAMLHelpers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Interop;
    using System.Windows.Markup;
    using System.Windows.Media.Imaging;
    using System.Xml;

    public class TextBlockFormatter
    {
        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
        "FormattedText",
        typeof(string),
        typeof(TextBlockFormatter),
        new PropertyMetadata(null, FormattedTextPropertyChanged));

        public static void SetFormattedText(DependencyObject textBlock, string value)
        {
            textBlock.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(DependencyObject textBlock)
        {
            return (string)textBlock.GetValue(FormattedTextProperty);
        }

        private static void FormattedTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Clear current textBlock
            TextBlock textBlock = d as TextBlock;
            textBlock.ClearValue(TextBlock.TextProperty);
            textBlock.Inlines.Clear();
            // Create new formatted text
            string formattedText = (string)e.NewValue ?? string.Empty;
            string @namespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            if(formattedText.Contains("InlineUIContainer"))
            {
                try
                {
                    var isBase64 = formattedText.IndexOf("Base64") > 0;
                    var result = isBase64 ? new InlineUIContainer() { Child = new Image() { Height=250 } } : (InlineUIContainer)XamlReader.Parse(formattedText);
                    var image = result.Child as Image;
                    var source = image?.Source;
                    if(source == null)
                    {
                        // grab image uri
                        Regex regex = new Regex("Source=\"(.*)\" Height=");
                        var imageLoc = regex.Match(formattedText).Groups[1].ToString();

                        if(!string.IsNullOrEmpty(imageLoc))
                        {
                            try
                            {
                                var imageExists = !isBase64 && File.Exists(imageLoc);
                                using (Pfim.IImage pfimImage = imageExists ? Pfim.Pfimage.FromFile(imageLoc) : Pfim.Pfimage.FromStream(new MemoryStream(Convert.FromBase64String(imageLoc))))
                                {
                                    // pin image data to prevent AccessViolationException do to GC
                                    var handle = GCHandle.Alloc(pfimImage.Data, GCHandleType.Pinned);

                                    try
                                    {
                                        var data = Marshal.UnsafeAddrOfPinnedArrayElement(pfimImage.Data, 0);
                                        var bitmap = new System.Drawing.Bitmap(pfimImage.Width, pfimImage.Height, pfimImage.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, data);
                                        var hBitmap = bitmap.GetHbitmap();
                                        var bitmapImage = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                                        image.Source = bitmapImage;
                                    }
                                    finally
                                    {
                                        handle.Free();
                                    }
                                }
                            }
                            catch { }
                            textBlock.Inlines.Add(result);
                        }
                    }
                    else
                    {
                        textBlock.Inlines.Add(result);
                    }
                }
                catch { }
            }
            else
            {
                formattedText = new string(formattedText.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray());
                formattedText = $@"<Span xml:space=""preserve"" xmlns=""{@namespace}"">{formattedText}</Span>";
                // Inject to inlines
                var result = (Span)XamlReader.Parse(formattedText);
                textBlock.Inlines.Add(result);
            }
        }

    }
}
