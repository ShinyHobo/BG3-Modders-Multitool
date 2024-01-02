namespace bg3_modders_multitool.Views.Other
{
    using bg3_modders_multitool.Models;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// Interaction logic for FileLintingWindow.xaml
    /// </summary>
    public partial class FileLintingWindow : Window
    {
        public FileLintingWindow(List<LintingError> errors)
        {
            InitializeComponent();
            errorList.ItemsSource = errors;
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
