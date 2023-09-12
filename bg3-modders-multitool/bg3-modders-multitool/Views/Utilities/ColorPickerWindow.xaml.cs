namespace bg3_modders_multitool.Views.Utilities
{
    using bg3_modders_multitool.Services;
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
            sARGB.Content = "0.00000000 0.00000000 0.00000000 0.00000000";
        }

        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            var selectedColor = colorPicker.SelectedColor.Value;

            hex.Content = selectedColor.ToString();

            var scA = selectedColor.ScA.ToString("0.00000000");
            var scR = selectedColor.ScR.ToString("0.00000000");
            var scG = selectedColor.ScG.ToString("0.00000000");
            var scB = selectedColor.ScB.ToString("0.00000000");
            sARGB.Content = $"{scA} {scR} {scG} {scB}";
        }

        private void hex_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetDataObject(hex.Content, false, 10, 10);
            GeneralHelper.WriteToConsole(Properties.Resources.CopiedToClipboard, hex.Content);
        }

        private void sARGB_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetDataObject(sARGB.Content, false, 10, 10);
            GeneralHelper.WriteToConsole(Properties.Resources.CopiedToClipboard, sARGB.Content);
        }
    }
}
