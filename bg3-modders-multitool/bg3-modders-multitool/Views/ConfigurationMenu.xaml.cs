/// <summary>
/// The configuration menu code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using System.Linq;
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

            var selectedLanguage = Properties.Settings.Default.selectedLanguage;
            selectedLanguage = string.IsNullOrEmpty(selectedLanguage) ? ViewModels.MainWindow.AvailableLanguages.First().Code : selectedLanguage;
            languageSelection.ItemsSource = ViewModels.MainWindow.AvailableLanguages;
            languageSelection.SelectedItem = ViewModels.MainWindow.AvailableLanguages.FirstOrDefault(l => l.Code == selectedLanguage);
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
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Bg3exeSelect_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.Bg3ExeLocation = vm.FileLocationDialog("bg3Exe", "Select bg3.exe or bg3_dx11.exe location");
        }

        /// <summary>
        /// Opens dialog for selecting game's document folder location.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameDocumentsLocationSelect_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.GameDocumentsLocation = vm.FolderLocationDialog(nameof(Properties.Settings.gameDocumentsPath), "Select your game's documents folder location");
        }

        /// <summary>
        /// Activates the quick launch features.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.ToggleQuickLaunch(true);
        }

        /// <summary>
        /// Deactivates the quick launch features.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.ToggleQuickLaunch(false);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.ConfigOpen = false;
        }

        /// <summary>
        /// Reloads the language the application uses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Language_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedLanguage = (ViewModels.MainWindow.Language)languageSelection.SelectedItem;
            var vm = DataContext as ViewModels.MainWindow;
            vm.ReloadLanguage(selectedLanguage.Code);
        }
    }
}
