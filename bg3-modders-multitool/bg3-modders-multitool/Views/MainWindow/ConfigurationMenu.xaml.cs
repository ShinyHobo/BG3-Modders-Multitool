/// <summary>
/// The configuration menu code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using bg3_modders_multitool.Services;
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

            Title = $"{Properties.Resources.ConfigurationTitle} - {GeneralHelper.GetAppVersion()}";

            DataContext = mainWindow;
            ((ViewModels.MainWindow)DataContext).ConfigOpen = true;

            var selectedLanguage = ViewModels.MainWindow.GetSelectedLanguage();
            languageSelection.ItemsSource = ViewModels.MainWindow.AvailableLanguages;
            languageSelection.SelectedItem = ViewModels.MainWindow.AvailableLanguages.FirstOrDefault(l => l.Code == selectedLanguage.Code);
        }

        /// <summary>
        /// Opens dialog for selecting bg3.exe location.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Bg3exeSelect_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.Bg3ExeLocation = vm.FileLocationDialog("bg3Exe", Properties.Resources.SelectGameLocation);
        }

        /// <summary>
        /// Opens dialog for selecting game's document folder location.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameDocumentsLocationSelect_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.GameDocumentsLocation = vm.FolderLocationDialog(nameof(Properties.Settings.gameDocumentsPath), Properties.Resources.SelectGameDocLocation);
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

        #region Configuration
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

        private void UnlockThreads_Checked(object sender, RoutedEventArgs e)
        {
            GeneralHelper.ToggleUnlockThreads(true);
        }

        private void UnlockThreads_Unchecked(object sender, RoutedEventArgs e)
        {
            GeneralHelper.ToggleUnlockThreads(false);
        }

        private void PakToMods_Checked(object sender, RoutedEventArgs e)
        {
            GeneralHelper.TogglePakToMods(true);
        }

        private void PakToMods_Unchecked(object sender, RoutedEventArgs e)
        {
            GeneralHelper.TogglePakToMods(false);
        }
        #endregion
    }
}
