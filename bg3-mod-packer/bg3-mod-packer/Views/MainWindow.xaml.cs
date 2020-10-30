namespace bg3_mod_packer.Views
{
    using bg3_mod_packer.Helpers;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // TODO move to model
            divineLocation.Text = Properties.Settings.Default.divineExe;
            divineLocation.ToolTip = divineLocation.Text;
            bg3exeLocation.Text = Properties.Settings.Default.bg3Exe;
            bg3exeLocation.ToolTip = bg3exeLocation.Text;
            DataContext = new Models.MainWindow();
        }

        // TODO add TextChanged events to save as well
        private void DivineSelect_Click(object sender, RoutedEventArgs e)
        {
            FileLocationDialog(divineLocation, "divineExe");
        }

        private void Bg3exeSelect_Click(object sender, RoutedEventArgs e)
        {
            FileLocationDialog(bg3exeLocation, "bg3Exe");
        }

        private void ConsoleScroller_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (ConsoleScroller.VerticalOffset == ConsoleScroller.ScrollableHeight && e.ExtentHeightChange != 0)
            {   
                ConsoleScroller.ScrollToEnd();
            }
        }

        // TODO move to viewmodel
        private void FileLocationDialog(System.Windows.Controls.TextBox location, string property)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    location.Text = file;
                    location.ToolTip = location.Text;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    location.Text = null;
                    location.ToolTip = location.Text;
                    break;
            }
            Properties.Settings.Default[property] = location.Text;
            Properties.Settings.Default.Save();
        }

        private void UnpackAll_Click(object sender, RoutedEventArgs e)
        {
            ((Models.MainWindow)this.DataContext).UnpackingProcess = new PakUnpackHelper();
            ((Models.MainWindow)this.DataContext).UnpackingProcess.UnpackAllPakFiles();
        }

        private void UnpackAll_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((Models.MainWindow)this.DataContext).UnpackingProcess.CancelUpacking();
        }

        private void IndexFiles_Click(object sender, RoutedEventArgs e)
        {
            var unpackPath = $"{Properties.Settings.Default.divineExe}\\..\\UnpackedData";
            var fileList = IndexHelper.DirectorySearch(unpackPath);
            new System.Threading.Tasks.Task(() => { IndexHelper.Index(fileList); }).Start();
        }
    }
}
