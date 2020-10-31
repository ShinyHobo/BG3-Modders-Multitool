namespace bg3_mod_packer.Views
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;

    /// <summary>
    /// Interaction logic for PakSelection.xaml
    /// </summary>
    public partial class PakSelection : Window
    {
        public PakSelection()
        {
            InitializeComponent();
            DataContext = new Models.PakSelection();
        }

        public PakSelection(List<string> files) : this()
        {
            var pakList = new ObservableCollection<Models.CheckBox>();
            foreach(string file in files)
            {
                pakList.Add(new Models.CheckBox {
                    Name = Path.GetFileName(file),
                    IsSelected = false
                });
            }
            ((Models.PakSelection)DataContext).PakList = pakList;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((Models.PakSelection)DataContext).PakList = new ObservableCollection<Models.CheckBox>();
            Close();
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var pak in ((Models.PakSelection)DataContext).PakList)
            {
                pak.IsSelected = false;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach(var pak in ((Models.PakSelection)DataContext).PakList)
            {
                pak.IsSelected = true;
            }
        }
    }
}
