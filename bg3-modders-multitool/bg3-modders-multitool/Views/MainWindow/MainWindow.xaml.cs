/// <summary>
/// The main window code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using bg3_modders_multitool.Services;
    using bg3_modders_multitool.Views.Utilities;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Explicitly set the translation to use
            var selectedLanguage = Properties.Settings.Default.selectedLanguage;
            System.Threading.Thread.CurrentThread.CurrentUICulture = string.IsNullOrEmpty(selectedLanguage) ? CultureInfo.InvariantCulture : new CultureInfo(selectedLanguage);

            InitializeComponent();
            DataContext = new ViewModels.MainWindow
            {
                DragAndDropBox = (ViewModels.DragAndDropBox)dragAndDropBox.DataContext,
                SearchResults = new ViewModels.SearchResults()
            };
        }

        #region File Unpacker
        /// <summary>
        /// Opens dialog for selecting and unpacking .pak game assets.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private async void Unpack_Click(object sender, RoutedEventArgs e)
        {
            unpack.Visibility = Visibility.Hidden;
            unpack_Cancel.Visibility = Visibility.Visible;
            var vm = DataContext as ViewModels.MainWindow;
            await vm.Unpacker.UnpackSelectedPakFiles().ContinueWith(delegate {
                Application.Current.Dispatcher.Invoke(() => {
                    unpack.Visibility = Visibility.Visible;
                    unpack_Cancel.Visibility = Visibility.Hidden;
                });
            });
        }

        /// <summary>
        /// Cancels the current process set for unpacking .pak game assets.
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Unpack_Cancel_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.Unpacker.Cancelled = true;
            unpack.Visibility = Visibility.Visible;
            unpack_Cancel.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Scrolls to the bottom of the console window on update if already scrolled to the bottom.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The scroll changed event arguments.</param>
        private void ConsoleScroller_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (ConsoleScroller.VerticalOffset == ConsoleScroller.ScrollableHeight && e.ExtentHeightChange != 0)
            {
                ConsoleScroller.ScrollToEnd();
            }
        }
        #endregion

        #region UUID Generation
        private void GuidGenerate_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.GenerateGuid(typeSwitch.IsChecked??false);
        }

        private void GuidText_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.CopyGuid(typeSwitch.IsChecked ?? false);
        }
        #endregion

        #region Indexing
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            new IndexingWindow().Show();
        }

        private void IndexFiles_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.Forms.DialogResult.OK;
            if(IndexHelper.IndexDirectoryExists())
            {
                result = System.Windows.Forms.MessageBox.Show(Properties.Resources.ReindexQuestion, Properties.Resources.ReadyToIndexAgainQuestion, System.Windows.Forms.MessageBoxButtons.OKCancel);
            }

            if(result.Equals(System.Windows.Forms.DialogResult.OK))
            {
                var vm = DataContext as ViewModels.MainWindow;
                vm.SearchResults.IndexHelper.Index();
            }
        }
        #endregion

        private void LaunchGameButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            var dataDir = FileHelper.DataDirectory;
            if (Directory.Exists(dataDir))
            {
                System.Diagnostics.Process.Start(vm.Bg3ExeLocation, Properties.Settings.Default.quickLaunch ? "-continueGame --skip-launcher" : string.Empty);
            }
            else
            {
                GeneralHelper.WriteToConsole(Properties.Resources.InvalidBg3LocationSelection);
            }
        }

        private void GameObjectButton_Click(object sender, RoutedEventArgs e)
        {
            new GameObjectWindow().Show();
        }

        private void Decompress_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            if(vm.NotDecompressing)
            {
                var result = System.Windows.Forms.MessageBox.Show(Properties.Resources.DecompressQuestion, Properties.Resources.DecompressQuestionTitle, System.Windows.Forms.MessageBoxButtons.YesNo);
                if(result == System.Windows.Forms.DialogResult.Yes)
                {
                    vm.NotDecompressing = false;
                    vm.SearchResults.PakUnpackHelper.DecompressAllConvertableFiles().ContinueWith(delegate {
                        Application.Current.Dispatcher.Invoke(() => {
                            vm.NotDecompressing = true;
                        });
                    });
                }
            }
        }

        private void configMenu_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            if (!vm.ConfigOpen)
            {
                var config = new ConfigurationMenu(vm);
                try
                {
                    config.Owner = this;
                    config.Show();
                } catch { }
            }
        }

        #region Shortcuts Tab
        /// <summary>
        /// Opens the mods folder in the file explorer. 
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OpenModsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(PathHelper.ModsFolderPath);
        }

        /// <summary>
        /// Opens the player profiles folder in the file explorer. 
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OpenProfilesFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(PathHelper.PlayerProfilesFolderPath);
        }

        /// <summary>
        /// Creates and opens the temp folder
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TempFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(DragAndDropHelper.TempFolder);
            System.Diagnostics.Process.Start(DragAndDropHelper.TempFolder);
        }

        /// <summary>
        /// Creates and opens the UnpackedData folder
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void unpackedModsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(FileHelper.UnpackedModsPath);
            System.Diagnostics.Process.Start(FileHelper.UnpackedModsPath);
        }

        /// <summary>
        /// Creates and opens the UnpackedMods folder
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void unpackedDataFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(FileHelper.UnpackedDataPath);
            System.Diagnostics.Process.Start(FileHelper.UnpackedDataPath);
        }

        /// <summary>
        /// Opens the Game Data location
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void gameDataFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dataDir = FileHelper.DataDirectory;
            if (Directory.Exists(dataDir))
            {
                System.Diagnostics.Process.Start(dataDir);
            }
            else
            {
                GeneralHelper.WriteToConsole(Properties.Resources.InvalidBg3Location);
            }
        }
        #endregion

        private void gameObjectCacheClearButton_Click(object sender, RoutedEventArgs e)
        {
            RootTemplateHelper.ClearGameObjectCache();
        }

        #region Help Tab
        #region Links
        private void BG3WikiLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://bg3.wiki/wiki/Modding_Resources");
        }

        private void BG3CommWikiLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/BG3-Community-Library-Team/BG3-Community-Library/wiki");
        }

        private void ModTutLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.nexusmods.com/baldursgate3/mods/1514");
        }

        private void BG3SELink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Norbyte/bg3se/releases");
        }

        private void BG3SEAPILink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Norbyte/bg3se/blob/main/Docs/API.md");
        }

        private void BG3SESampleLinkClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Norbyte/bg3se/tree/main/SampleMod");
        }

        private void LuaSetupLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/LaughingLeader/BG3ModdingTools/wiki/Script-Extender-Lua-Setup");
        }

        private void ReportABugLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ShinyHobo/BG3-Modders-Multitool/issues");
        }

        private void KofiLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://ko-fi.com/shinyhobo");
        }

        private void LSLibLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Norbyte/lslib/releases");
        }
        #endregion

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.CheckForUpdates();
        }

        /// <summary>
        /// Opens the wiki
        /// </summary>
        private void HowToUse_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ShinyHobo/BG3-Modders-Multitool/wiki");
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Opens dialog to select paks to unpack
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UnpackMod_Click(object sender, RoutedEventArgs e)
        {
            var unpackPakDialog = new System.Windows.Forms.OpenFileDialog() {
                Filter = $"{Properties.Resources.PakFileDescription}|*.pak",
                Title = Properties.Resources.UnpackModPaks,
                DefaultExt = ".pak",
                Multiselect = true,
                CheckFileExists = true
            };
            var result = unpackPakDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DragAndDropHelper.CleanTempDirectory();

                var vm = DataContext as ViewModels.MainWindow;
                await vm.Unpacker.UnpackPakFiles(unpackPakDialog.FileNames.ToList(), false).ContinueWith(delegate {
                    Application.Current.Dispatcher.Invoke(() => {
                        unpack.Visibility = Visibility.Visible;
                        unpack_Cancel.Visibility = Visibility.Hidden;
                    });
                });
            }
        }

        /// <summary>
        /// Opens the atlas tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void atlasToolButton_Click(object sender, RoutedEventArgs e)
        {
            var atlasToolDialog = new AtlasToolWindow();
            atlasToolDialog.Show();
        }

        /// <summary>
        /// Opens the color picker window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void colorPicker_Click(object sender, RoutedEventArgs e)
        {
            var colorPickerWIndow = new ColorPickerWindow();
            colorPickerWIndow.Show();
        }
        #endregion

        #region About menu
        private void LegalMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(Properties.Resources.Copyright, Properties.Resources.LegalMenu, System.Windows.Forms.MessageBoxButtons.OK);
        }

        private void CheckForUpdateMenu_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.CheckForUpdates(true);
        }
        #endregion
    }
}
