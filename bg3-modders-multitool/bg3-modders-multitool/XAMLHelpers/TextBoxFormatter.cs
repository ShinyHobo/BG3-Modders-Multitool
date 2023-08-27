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
    using System.Windows.Media;
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
                    var result = (InlineUIContainer)XamlReader.Parse(formattedText);
                    var image = result.Child as Image;
                    var source = image.Source;
                    if(source == null)
                    {
                        // grab image uri
                        Regex regex = new Regex("Source=\"(.*)\" Height=");
                        var imageLoc = regex.Match(formattedText).Groups[1].ToString();

                        try
                        {
                            // convert image to something WPF can read
                            using (var image2 = Pfim.Pfimage.FromFile(imageLoc))
                            {
                                var data = Marshal.UnsafeAddrOfPinnedArrayElement(image2.Data, 0);
                                var bitmap = new System.Drawing.Bitmap(image2.Width, image2.Height, image2.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, data);
                                var bitmapImage = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                                image.Source = bitmapImage;
                            }
                        } catch { }
                        textBlock.Inlines.Add(result);
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
