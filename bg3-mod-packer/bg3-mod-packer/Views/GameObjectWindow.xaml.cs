namespace bg3_mod_packer.Views
{
    using bg3_mod_packer.Services;
    using System.Windows;

    /// <summary>
    /// Interaction logic for GameObjectWindow.xaml
    /// </summary>
    public partial class GameObjectWindow : Window
    {
        public GameObjectWindow()
        {
            InitializeComponent();
            DataContext = new RootTemplateHelper();
        }

        private void CharacterButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as RootTemplateHelper;
            vm.LoadRelevent("character");
        }

        private void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as RootTemplateHelper;
            vm.LoadRelevent("item");
        }
    }
}
