namespace bg3_modders_multitool.Views.Utilities
{
    using bg3_modders_multitool.ViewModels.Utilities;
    using LSLib.LS;
    using System.Windows;

    /// <summary>
    /// Interaction logic for AddMissingMetaLsx.xaml
    /// </summary>
    public partial class AddMissingMetaLsx : Window
    {
        public string MetaPath { get; private set; }

        /// <summary>
        /// Popup for adding missing meta.lsx information
        /// </summary>
        /// <param name="modPath">The mod path</param>
        public AddMissingMetaLsx(string modPath)
        {
            InitializeComponent();
            DataContext = new AddMissingMetaLsxViewModel(modPath);
        }

        private void accept_Click(object sender, RoutedEventArgs e)
        {
            var version = new PackedVersion() {
                Major = (uint)major.Value,
                Minor = (uint)minor.Value,
                Build = (uint)build.Value,
                Revision = (uint)revision.Value
            };

            var vm = DataContext as AddMissingMetaLsxViewModel;
            vm.GenerateMetaLsx(author.Text.Trim(), description.Text.Trim(), version);
            MetaPath = vm.MetaPath;
            DialogResult = true;
            Close();
        }
    }
}
