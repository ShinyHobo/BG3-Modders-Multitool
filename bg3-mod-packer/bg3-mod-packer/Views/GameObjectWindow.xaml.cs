namespace bg3_mod_packer.Views
{
    using bg3_mod_packer.Services;
    using System.Linq;
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
            DisplayTree(characters, "Characters");
            ToggleButtons(sender);
        }

        private async void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButtons();
            treeView.Items.Clear();
            var vm = DataContext as RootTemplateHelper;
            var items = await vm.LoadRelevent("item");
            DisplayTree(items, "Items");
            ToggleButtons(sender);
        }

        private void Collapsed(object sender, RoutedEventArgs e)
        {
            (sender as TreeViewItem).IsExpanded = true;
        }

        /// <summary>
        /// Generates the treeview.
        /// </summary>
        /// <param name="gameObjects">The list of game objects to display.</param>
        /// <param name="type">The type of game objects being displayed.</param>
        private void DisplayTree(System.Collections.Generic.List<Models.GameObject> gameObjects, string type)
        {
            var all = new TreeViewItem
            {
                Header = $"All {type} ({gameObjects.Sum(x => x.Count) + gameObjects.Count})",
                IsExpanded = true
            };
            all.Collapsed += Collapsed;
            AddTreeViewItems(gameObjects, all);
            treeView.Items.Add(all);
        }

        /// <summary>
        /// Recursively adds game objects to the treeview.
        /// </summary>
        /// <param name="gameObjects">The game objects to add.</param>
        /// <param name="treeViewItem">The treeview item to append to.</param>
        private void AddTreeViewItems(System.Collections.Generic.List<Models.GameObject> gameObjects, TreeViewItem treeViewItem)
        {
            foreach (var gameObject in gameObjects)
            {
                var infoContents = new StackPanel { Orientation = Orientation.Vertical };
                infoContents.Children.Add(new TextBox { Text = $"Name: {gameObject.Name}", BorderThickness = new Thickness(0,0,0,0), IsReadOnly = true });
                infoContents.Children.Add(new TextBox { Text = $"DisplayName: {gameObject.DisplayName}", BorderThickness = new Thickness(0, 0, 0, 0), IsReadOnly = true });
                infoContents.Children.Add(new TextBox { Text = $"Description: {gameObject.Description}", BorderThickness = new Thickness(0, 0, 0, 0), IsReadOnly = true });
                infoContents.Children.Add(new TextBox { Text = $"MapKey: {gameObject.MapKey}", BorderThickness = new Thickness(0, 0, 0, 0), IsReadOnly = true });
                infoContents.Children.Add(new TextBox { Text = $"ParentTemplateId: {gameObject.ParentTemplateId}", BorderThickness = new Thickness(0, 0, 0, 0), IsReadOnly = true });
                infoContents.Children.Add(new TextBox { Text = $"Stats: {gameObject.Stats}", BorderThickness = new Thickness(0, 0, 0, 0), IsReadOnly = true });
                var info = new TreeViewItem { Header = "Info" };
                info.Items.Add(infoContents);
                var item = new TreeViewItem { Header = gameObject.Name };
                item.Items.Add(info);
                AddTreeViewItems(gameObject.Children, item);
                treeViewItem.Items.Add(item);
            }
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
