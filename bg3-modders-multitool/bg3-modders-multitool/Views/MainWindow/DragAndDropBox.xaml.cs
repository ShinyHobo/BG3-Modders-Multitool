/// <summary>
/// The drag and drop box code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using Ookii.Dialogs.Wpf;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for DragAndDropBox.xaml
    /// </summary>
    public partial class DragAndDropBox : UserControl
    {
        private bool rectMouseDown = false;
        private ViewModels.DragAndDropBox vm;

        public DragAndDropBox()
        {
            InitializeComponent();
            vm = new ViewModels.DragAndDropBox();
            DataContext = vm;
        }

        /// <summary>
        /// Process a drop.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected async override void OnDrop(DragEventArgs e)
        {
            await vm.ProcessDrop(e.Data);
            var fileDrop = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            if(fileDrop != null)
            {
                if (Directory.Exists(fileDrop[0]))
                {
                    vm.LastDirectory = fileDrop[0];
                }
            }
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            vm.Darken();
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            vm.Lighten();
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rectMouseDown = false;
        }

        private async void OnClick()
        {
            var vm = DataContext as ViewModels.DragAndDropBox;
            var folderDialog = new VistaFolderBrowserDialog()
            {
                Description = Properties.Resources.PleaseSelectWorkspace,
                SelectedPath = string.IsNullOrEmpty(vm.LastDirectory) ? Alphaleonis.Win32.Filesystem.Directory.GetCurrentDirectory() : vm.LastDirectory,
                UseDescriptionForTitle = true
            };

            if(folderDialog.ShowDialog() == true)
            {
                vm.LastDirectory = folderDialog.SelectedPath;
                DataObject data = new DataObject(DataFormats.FileDrop, new string[] { folderDialog.SelectedPath });
                await vm.ProcessDrop(data);
            }
        }

        private void Rectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!vm.PackAllowed)
                return;

            rectMouseDown = true;
        }

        private void Rectangle_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
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
