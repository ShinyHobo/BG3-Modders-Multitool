/// <summary>
/// The configuration menu code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using bg3_modders_multitool.Services;
    using bg3_modders_multitool.Themes;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Interaction logic for ConfigurationMenu.xaml
    /// </summary>
    public partial class ConfigurationMenu : Window
    {
        private ViewModels.MainWindow vm;
        public ConfigurationMenu(ViewModels.MainWindow mainWindow)
        {
            InitializeComponent();

            Title = $"{Properties.Resources.ConfigurationTitle} - {GeneralHelper.GetAppVersion()}";

            vm = mainWindow;
            DataContext = vm;
            ((ViewModels.MainWindow)DataContext).ConfigOpen = true;

            var selectedLanguage = ViewModels.MainWindow.GetSelectedLanguage();
            languageSelection.ItemsSource = ViewModels.MainWindow.AvailableLanguages;
            languageSelection.SelectedItem = ViewModels.MainWindow.AvailableLanguages.FirstOrDefault(l => l.Code == selectedLanguage.Code);

            var selectedTheme = (ThemeType)Properties.Settings.Default.theme;
            var themes = new List<ViewModels.MainWindow.Theme>() {
                new ViewModels.MainWindow.Theme(ThemeType.LightTheme.GetName(), ThemeType.LightTheme),
                new ViewModels.MainWindow.Theme(ThemeType.SoftDark.GetName(), ThemeType.SoftDark),
                new ViewModels.MainWindow.Theme(ThemeType.DeepDark.GetName(), ThemeType.DeepDark),
                new ViewModels.MainWindow.Theme(ThemeType.RedBlackTheme.GetName(), ThemeType.RedBlackTheme)
            };

            themeSelection.ItemsSource = themes;
            themeSelection.SelectedIndex = themes.FindIndex(t => t.Type == selectedTheme);
        }

        /// <summary>
        /// Opens dialog for selecting bg3.exe location.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Bg3exeSelect_Click(object sender, RoutedEventArgs e)
        {
            vm.Bg3ExeLocation = vm.FileLocationDialog("bg3Exe", Properties.Resources.SelectGameLocation);
        }

        /// <summary>
        /// Opens dialog for selecting game's document folder location.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameDocumentsLocationSelect_Click(object sender, RoutedEventArgs e)
        {
            vm.GameDocumentsLocation = vm.FolderLocationDialog(nameof(Properties.Settings.gameDocumentsPath), Properties.Resources.SelectGameDocLocation);
        }

        /// <summary>
        /// Activates the quick launch features.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            vm.ToggleQuickLaunch(true);
        }

        /// <summary>
        /// Deactivates the quick launch features.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            vm.ToggleQuickLaunch(false);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
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
            vm.ReloadLanguage(selectedLanguage.Code);
        }

        /// <summary>
        /// Selects the theme to use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void themeSelection_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedTheme = themeSelection.SelectedItem as ViewModels.MainWindow.Theme;
            Properties.Settings.Default.theme = (int)selectedTheme.Type;
            Properties.Settings.Default.Save();
            ThemesController.SetTheme(selectedTheme.Type);
        }

        private void UnlockThreads_Checked(object sender, RoutedEventArgs e)
        {
            GeneralHelper.ToggleUnlockThreads(true);
        }

        private void UnlockThreads_Unchecked(object sender, RoutedEventArgs e)
        {
            GeneralHelper.ToggleUnlockThreads(false);
        }
        #endregion
    }
}
