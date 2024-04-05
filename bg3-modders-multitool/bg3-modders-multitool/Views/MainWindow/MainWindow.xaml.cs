/// <summary>
/// The main window code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using bg3_modders_multitool.Services;
    using bg3_modders_multitool.ViewModels;
    using bg3_modders_multitool.Views.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModels.MainWindow MainWindowVM;

        public MainWindow()
        {
            // Explicitly set the translation to use
            var selectedLanguage = Properties.Settings.Default.selectedLanguage;
            System.Threading.Thread.CurrentThread.CurrentUICulture = string.IsNullOrEmpty(selectedLanguage) ? CultureInfo.InvariantCulture : new CultureInfo(selectedLanguage);

            InitializeComponent();

            MainWindowVM = new ViewModels.MainWindow
            {
                DragAndDropBox = (ViewModels.DragAndDropBox)dragAndDropBox.DataContext,
                SearchResults = new SearchResults()
            };

            DataContext = MainWindowVM;

            MainWindowVM.SearchResults.AllowIndexing = true;

            // Check for install integrity
            if (!FileHelper.IsDirectoryWritable(Directory.GetCurrentDirectory()))
            {
                GeneralHelper.WriteToConsole(Properties.Resources.NoWriteAccess);
            }
            #if DEBUG == false
            if (File.Exists($"{Directory.GetCurrentDirectory()}\\LSLib.dll"))
            {
                GeneralHelper.WriteToConsole(Properties.Resources.ExternalLSLibFound);
            }
            #endif

            if(GeneralHelper.IsAdministrator)
            {
                GeneralHelper.WriteToConsole(Properties.Resources.RunningAsAdmin);
            }

            compressionOption.ItemsSource = ViewModels.MainWindow.AvailableCompressionTypes;
            compressionOption.SelectedIndex = Properties.Settings.Default.packingCompressionOption;
        }

        #region General
        /// <summary>
        /// Launches the game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchGameButton_Click(object sender, RoutedEventArgs e)
        {
            var dataDir = FileHelper.DataDirectory;
            if (Directory.Exists(dataDir)&&File.Exists(MainWindowVM.Bg3ExeLocation))
            {
                var bg3Exe = new ProcessStartInfo(MainWindowVM.Bg3ExeLocation);
                bg3Exe.Arguments = Properties.Settings.Default.quickLaunch ? "-continueGame --skip-launcher" : string.Empty;
                bg3Exe.WorkingDirectory = Directory.GetParent(MainWindowVM.Bg3ExeLocation).FullName;
                var pr = new Process();
                pr.StartInfo = bg3Exe;
                pr.Start();
            }
            else
            {
                GeneralHelper.WriteToConsole(Properties.Resources.InvalidBg3LocationSelection);
            }
        }

        /// <summary>
        /// Opens the configuration menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                }
                catch { }
            }
        }
        #endregion

        #region File Unpacker
        /// <summary>
        /// Opens dialog for selecting and unpacking .pak game assets.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private async void Unpack_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            await vm.Unpacker.UnpackSelectedPakFiles();
        }

        /// <summary>
        /// Cancels the current process set for unpacking .pak game assets.
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Unpack_Cancel_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            vm.Unpacker.Cancelled = true;
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
            if (!IndexHelper.IndexDirectoryExists())
            {
                GeneralHelper.WriteToConsole(Properties.Resources.IndexNotFoundGenerating);
                IndexFiles(true).ContinueWith((i) => {
                    Application.Current.Dispatcher.Invoke(() => { new IndexingWindow().Show(); });
                });
            }
            else
            {
                new IndexingWindow().Show();
            }
        }

        private void IndexFiles_Click(object sender, RoutedEventArgs e)
        {
            IndexFiles(false);
        }

        private void indexPakFiles_Click(object sender, RoutedEventArgs e)
        {
            IndexFiles(true);
        }

        /// <summary>
        /// Creates a confirmation prompt
        /// On accept, deletes the index and generates a new one
        /// </summary>
        /// <param name="directIndex">Whether or not to index the pak files directly, or index the files inside UnpackedData (legacy mode)</param>
        private Task IndexFiles(bool directIndex)
        {
            return Task.Run(() => {
                if(directIndex)
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        await MainWindowVM.SearchResults.IndexHelper.IndexDirectly();
                    }).Wait();
                }
                else
                {
                    var result = System.Windows.Forms.DialogResult.OK;
                    if (IndexHelper.IndexDirectoryExists())
                    {
                        result = System.Windows.Forms.MessageBox.Show(Properties.Resources.ReindexQuestion, Properties.Resources.ReadyToIndexAgainQuestion, System.Windows.Forms.MessageBoxButtons.OKCancel);
                    }

                    if (result.Equals(System.Windows.Forms.DialogResult.OK))
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            await MainWindowVM.SearchResults.IndexHelper.Index();
                        }).Wait();
                    }
                }
            });
        }

        /// <summary>
        /// Delete the index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteIndex_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => {
                var result = System.Windows.Forms.DialogResult.OK;
                if (IndexHelper.IndexDirectoryExists())
                {
                    result = System.Windows.Forms.MessageBox.Show(Properties.Resources.ClearIndexQuestion, Properties.Resources.AreYouSure, System.Windows.Forms.MessageBoxButtons.OKCancel);
                }

                if (result.Equals(System.Windows.Forms.DialogResult.OK))
                {
                    MainWindowVM.SearchResults.IndexHelper.DeleteIndex();
                }
            });
        }
        #endregion

        #region Mod Packing

        private void PakToMods_Checked(object sender, RoutedEventArgs e)
        {
            GeneralHelper.TogglePakToMods(true);
        }

        private void PakToMods_Unchecked(object sender, RoutedEventArgs e)
        {
            GeneralHelper.TogglePakToMods(false);
        }

        /// <summary>
        /// Rebuilds the selected directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void rebuild_Click(object sender, RoutedEventArgs e)
        {
            DataObject data = new DataObject(DataFormats.FileDrop, new string[] { MainWindowVM.DragAndDropBox.LastDirectory });
            rebuildBtn.IsEnabled = false;
            // TODO - pass version here
            await MainWindowVM.DragAndDropBox.ProcessDrop(data);
            // TODO - auto increment revision here
            rebuildBtn.IsEnabled = true;
        }
        #endregion

        #region Shortcuts Tab
        /// <summary>
        /// Opens the mods folder in the file explorer. 
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OpenModsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart(PathHelper.ModsFolderPath);
        }

        /// <summary>
        /// Opens the player profiles folder in the file explorer. 
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OpenProfilesFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart(PathHelper.PlayerProfilesFolderPath);
        }

        /// <summary>
        /// Creates and opens the temp folder
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TempFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(DragAndDropHelper.TempFolder);
            FileHelper.ProcessStart(DragAndDropHelper.TempFolder);
        }

        /// <summary>
        /// Creates and opens the UnpackedData folder
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void unpackedModsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(FileHelper.UnpackedModsPath);
            FileHelper.ProcessStart(FileHelper.UnpackedModsPath);
        }

        /// <summary>
        /// Creates and opens the UnpackedMods folder
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void unpackedDataFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(FileHelper.UnpackedDataPath);
            FileHelper.ProcessStart(FileHelper.UnpackedDataPath);
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
                FileHelper.ProcessStart(dataDir);
            }
            else
            {
                GeneralHelper.WriteToConsole(Properties.Resources.InvalidBg3Location);
            }
        }
        #endregion

        #region Help Tab
        #region Links
        private void BG3WikiLink_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://bg3.wiki/wiki/Modding_Resources");
        }

        private void BG3CommWikiLink_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://github.com/BG3-Community-Library-Team/BG3-Community-Library/wiki");
        }

        private void ModTutLink_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://www.nexusmods.com/baldursgate3/mods/1514");
        }

        private void BG3SELink_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://github.com/Norbyte/bg3se/releases");
        }

        private void BG3SEAPILink_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://github.com/Norbyte/bg3se/blob/main/Docs/API.md");
        }

        private void BG3SESampleLinkClick(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://github.com/Norbyte/bg3se/tree/main/SampleMod");
        }

        private void LuaSetupLink_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://github.com/LaughingLeader/BG3ModdingTools/wiki/Script-Extender-Lua-Setup");
        }

        private void ReportABugLink_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://github.com/ShinyHobo/BG3-Modders-Multitool/issues");
        }

        private void KofiLink_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://ko-fi.com/shinyhobo");
        }

        private void LSLibLink_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://github.com/Norbyte/lslib/releases");
        }

        private void ConvertWemLink_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://katiefrogs.github.io/vgmstream-web/");
        }

        private void SpellGenAssistant_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.ProcessStart("https://github.com/Shimizoki/BG3-Spell-Generation-Assistant");
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
            FileHelper.ProcessStart("https://github.com/ShinyHobo/BG3-Modders-Multitool/wiki");
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Clears the GameObject Explorer Cache folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gameObjectCacheClearButton_Click(object sender, RoutedEventArgs e)
        {
            RootTemplateHelper.ClearGameObjectCache();
        }

        /// <summary>
        /// Opens the GameObject Explorer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GameObjectButton_Click(object sender, RoutedEventArgs e)
        {

            if (!IndexHelper.IndexDirectoryExists())
            {
                GeneralHelper.WriteToConsole(Properties.Resources.IndexNotFoundGenerating);
                await IndexFiles(true);
            }
            new GameObjectWindow().Show();
        }

        /// <summary>
        /// Decompresses the files located in UnpackedData
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Decompress_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            if (vm.NotDecompressing)
            {
                var result = System.Windows.Forms.MessageBox.Show(Properties.Resources.DecompressQuestion, Properties.Resources.DecompressQuestionTitle, System.Windows.Forms.MessageBoxButtons.YesNo);
                if (result == System.Windows.Forms.DialogResult.Yes)
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
                await vm.Unpacker.UnpackPakFiles(unpackPakDialog.FileNames.ToList(), false);
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

        /// <summary>
        /// Opens the version generator window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void versionGenerator_Click(object sender, RoutedEventArgs e)
        {
            var versionGenerator = new VersionCalculator();
            versionGenerator.Show();
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

        /// <summary>
        /// Updates the compression option setting
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void CompressionOption_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MainWindowVM.PackingCompressionOption = ((ViewModels.MainWindow.PackingCompression)compressionOption.SelectedItem).Id;
        }
    }
}
