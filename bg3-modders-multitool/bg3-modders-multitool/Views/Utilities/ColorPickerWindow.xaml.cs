namespace bg3_modders_multitool.Views.Utilities
{
    using bg3_modders_multitool.Services;
    using J2N;
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        public ColorPickerWindow()
        {
            InitializeComponent();
            hex.Content = "#00000000";
            sRGB.Content = "0.00000000 0.00000000 0.00000000";
        }

        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            var selectedColor = colorPicker.SelectedColor.Value;

            hex.Content = selectedColor.ToString();

            var scR = selectedColor.ScR.ToString("0.00000000");
            var scG = selectedColor.ScG.ToString("0.00000000");
            var scB = selectedColor.ScB.ToString("0.00000000");
            sRGB.Content = $"{scR} {scG} {scB}";
        }

        private void hex_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetDataObject(hex.Content, false, 10, 10);
            GeneralHelper.WriteToConsole(Properties.Resources.CopiedToClipboard, hex.Content);
        }

        private void sARGB_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetDataObject(sRGB.Content, false, 10, 10);
            GeneralHelper.WriteToConsole(Properties.Resources.CopiedToClipboard, sRGB.Content);
        }

        /// <summary>
        /// Previews sRGB value that is copied into the text field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sRGBpreview_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var bc = new BrushConverter();
            try
            {
                var sRGB = sRGBpreview.Text.Split(' ').Select(p => float.Parse(p)).ToArray();
                var color = Color.FromScRgb(1, sRGB[0], sRGB[1], sRGB[2]);
                previewColor.Background = new SolidColorBrush(color);
            }
            catch
            {
                previewColor.Background = (Brush)bc.ConvertFrom("#FFFFFFFF");
            }
        }
    }
}
