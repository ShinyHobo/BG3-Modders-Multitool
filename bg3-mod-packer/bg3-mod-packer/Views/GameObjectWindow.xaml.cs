namespace bg3_mod_packer.Views
{
    using bg3_mod_packer.Services;
    using System.Windows;
    using System.Windows.Controls;

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

        private async void CharacterButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButtons();
            treeView.Items.Clear();
            var vm = DataContext as RootTemplateHelper;
            var characters = await vm.LoadRelevent("character");
            foreach(var character in characters)
            {
                var item = new TreeViewItem
                {
                    Header = character.Name
                };
                treeView.Items.Add(item);
            }
            ToggleButtons(sender);
        }

        private async void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButtons();
            treeView.Items.Clear();
            var vm = DataContext as RootTemplateHelper;
            var items = await vm.LoadRelevent("item");
            foreach (var character in items)
            {
                var item = new TreeViewItem
                {
                    Header = character.Name
                };
                treeView.Items.Add(item);
            }
            ToggleButtons(sender);
        }

        /// <summary>
        /// Toggles buttons on or off.
        /// </summary>
        /// <param name="sender">The sender to ignore.</param>
        private void ToggleButtons(object sender = null)
        {
            var enable = sender != null;
            characterButton.IsEnabled = enable && characterButton != sender;
            itemButton.IsEnabled = enable && itemButton != sender;
        }
    }
}
