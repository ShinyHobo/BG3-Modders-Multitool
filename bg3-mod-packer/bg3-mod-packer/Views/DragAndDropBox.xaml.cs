using System.Windows;
using System.Windows.Controls;

namespace bg3_mod_packer.Views
{
    /// <summary>
    /// Interaction logic for DragAndDropBox.xaml
    /// </summary>
    public partial class DragAndDropBox : UserControl
    {
        public DragAndDropBox()
        {
            InitializeComponent();
        }

        public DragAndDropBox(DragAndDropBox c)
        {
            InitializeComponent();
        }

        protected override void OnDrop(DragEventArgs e)
        {
            ViewModels.DragAndDropBox.ProcessDrop(e.Data);
        }
    }
}
