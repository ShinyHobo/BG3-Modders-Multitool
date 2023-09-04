/// <summary>
/// The pak selection code behind.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// Interaction logic for PakSelection.xaml
    /// </summary>
    public partial class PakSelection : Window
    {
        public PakSelection()
        {
            InitializeComponent();
            DataContext = new ViewModels.PakSelection();
        }

        public PakSelection(List<string> files) : this()
        {
            var vm = DataContext as ViewModels.PakSelection;
            vm.CreateFileList(files);
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.PakSelection;
            vm.PakList.Clear();
            DialogResult = false;
            Close();
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.PakSelection;
            vm.SelectAll(false);
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.PakSelection;
            vm.SelectAll(true);
        }
    }
}
