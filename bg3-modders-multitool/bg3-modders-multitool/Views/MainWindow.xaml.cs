/// <summary>
/// The main window code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainWindow
            {
                DragAndDropBox = (ViewModels.DragAndDropBox)dragAndDropBox.DataContext,
                SearchResults = new ViewModels.SearchResults()
            };
        }

        #region File Selection
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
        #endregion

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
            await vm.Unpacker.UnpackAllPakFiles().ContinueWith(delegate {
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
            vm.Unpacker.CancelUpacking();
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

        private async void IndexFiles_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.Forms.DialogResult.OK;
            if(Services.IndexHelper.IndexDirectoryExists())
            {
                result = System.Windows.Forms.MessageBox.Show("Careful! \n\nClicking \"OK\" will wipe your current index and rebuild it from scratch; " +
                    "this could take some time. Are you sure you wish you continue?", "Ready to index again?", System.Windows.Forms.MessageBoxButtons.OKCancel);
            }

            if(result.Equals(System.Windows.Forms.DialogResult.OK))
            {
                var vm = DataContext as ViewModels.MainWindow;
                await vm.SearchResults.IndexHelper.Index();
            }
        }
        #endregion

        private void LaunchGameButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            System.Diagnostics.Process.Start(vm.Bg3ExeLocation);
        }

        private void GameObjectButton_Click(object sender, RoutedEventArgs e)
        {
            new GameObjectWindow().Show();
        }

        private async void Decompress_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.MainWindow;
            await vm.Unpacker.DecompressAllConvertableFiles();
        }
    }
}
