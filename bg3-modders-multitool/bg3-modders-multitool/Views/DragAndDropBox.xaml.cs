/// <summary>
/// The drag and drop box code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using bg3_modders_multitool.Properties;
    using Lucene.Net.Store;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

    /// <summary>
    /// Interaction logic for DragAndDropBox.xaml
    /// </summary>
    public partial class DragAndDropBox : UserControl
    {
        private bool rectMouseDown = false;
        private string lastDirectory;

        public DragAndDropBox()
        {
            InitializeComponent();
            DataContext = new ViewModels.DragAndDropBox();
        }

        public DragAndDropBox(DragAndDropBox c)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Process a drop.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected async override void OnDrop(DragEventArgs e)
        {
            var vm = DataContext as ViewModels.DragAndDropBox;
            await vm.ProcessDrop(e.Data);
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            var vm = DataContext as ViewModels.DragAndDropBox;
            vm.Darken();
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            var vm = DataContext as ViewModels.DragAndDropBox;
            vm.Lighten();
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rectMouseDown = false;
        }

        private async void OnClick()
        {
            var vm = DataContext as ViewModels.DragAndDropBox;
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = Properties.Resources.SelectGameModDirectory,
                SelectedPath = string.IsNullOrEmpty(lastDirectory) ? Alphaleonis.Win32.Filesystem.Directory.GetCurrentDirectory() : lastDirectory
            };

            if(folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lastDirectory = folderDialog.SelectedPath;
                DataObject data = new DataObject(DataFormats.FileDrop, new string[] { folderDialog.SelectedPath });
                await vm.ProcessDrop(data);
            }
        }

        private void Rectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = DataContext as ViewModels.DragAndDropBox;

            if (!vm.PackAllowed)
                return;

            rectMouseDown = true;
        }

        private void Rectangle_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = DataContext as ViewModels.DragAndDropBox;

            if (!vm.PackAllowed)
                return;

            if (rectMouseDown)
            {
                OnClick();
            }
            rectMouseDown = false;
        }
    }
}
