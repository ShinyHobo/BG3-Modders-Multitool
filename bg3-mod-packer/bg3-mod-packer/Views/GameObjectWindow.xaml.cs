namespace bg3_mod_packer.Views
{
    using bg3_mod_packer.Services;
    using System;
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
            var all = new TreeViewItem
            {
                Header = $"All Characters ({characters.Sum(x => x.Count) + characters.Count})",
                IsExpanded = true
            };
            all.Collapsed += All_Collapsed;
            AddTreeViewItems(characters, all);
            treeView.Items.Add(all);
            ToggleButtons(sender);
        }

        private async void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButtons();
            treeView.Items.Clear();
            var vm = DataContext as RootTemplateHelper;
            var items = await vm.LoadRelevent("item");
            var all = new TreeViewItem
            {
                Header = $"All Items ({items.Sum(x => x.Count) + items.Count})",
                IsExpanded = true
            };
            all.Collapsed += All_Collapsed;
            AddTreeViewItems(items, all);
            treeView.Items.Add(all);
            ToggleButtons(sender);
        }

        private void All_Collapsed(object sender, RoutedEventArgs e)
        {
            (sender as TreeViewItem).IsExpanded = true;
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
                var item = new TreeViewItem
                {
                    Header = gameObject.Name
                };
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
