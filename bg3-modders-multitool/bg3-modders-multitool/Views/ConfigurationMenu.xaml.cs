/// <summary>
/// The configuration menu code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for ConfigurationMenu.xaml
    /// </summary>
    public partial class ConfigurationMenu : Window
    {
        public ConfigurationMenu(ViewModels.MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = mainWindow;
            ((ViewModels.MainWindow)DataContext).ConfigOpen = true;
        }

        /// <summary>
        /// Opens dialog for selecting divine.exe location.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void DivineSelect_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.DivineLocation = vm.FileLocationDialog("divineExe", "Select divine.exe location");
        }

        /// <summary>
        /// Opens dialog for selecting bg3.exe location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The event arguments.</param>
        private void Bg3exeSelect_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.Bg3ExeLocation = vm.FileLocationDialog("bg3Exe", "Select bg3.exe or bg3_dx11.exe location");
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.ConfigOpen = false;
        }
    }
}
