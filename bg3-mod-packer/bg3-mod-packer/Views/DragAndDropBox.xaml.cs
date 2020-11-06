/// <summary>
/// The drag and drop box code behind.
/// </summary>
namespace bg3_mod_packer.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for DragAndDropBox.xaml
    /// </summary>
    public partial class DragAndDropBox : UserControl
    {
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
        protected override void OnDrop(DragEventArgs e)
        {
            Services.DragAndDropHelper.ProcessDrop(e.Data);
        }
    }
}
