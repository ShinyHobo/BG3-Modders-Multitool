/// <summary>
/// The drag and drop box code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using Microsoft.WindowsAPICodePack.Dialogs;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for DragAndDropBox.xaml
    /// </summary>
    public partial class DragAndDropBox : UserControl
    {
        private bool rectMouseDown = false;

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

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = string.Empty;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DataObject data = new DataObject(DataFormats.UnicodeText, dialog.FileName);
                await vm.ProcessClick(data);
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