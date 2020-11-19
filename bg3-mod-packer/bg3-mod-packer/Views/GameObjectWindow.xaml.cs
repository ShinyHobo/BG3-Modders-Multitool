namespace bg3_mod_packer.Views
{
    using bg3_mod_packer.ViewModels;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for GameObjectWindow.xaml
    /// </summary>
    public partial class GameObjectWindow : Window
    {
        public GameObjectWindow()
        {
            InitializeComponent();
            DataContext = new GameObjectViewModel();
        }

        private async void Type_Change(object sender, RoutedEventArgs e)
        {
            var combo = (ComboBox)sender;
            if(combo.SelectedIndex != 0)
            {
                searchBox.Text = string.Empty;
                ToggleControls();
                var vm = DataContext as GameObjectViewModel;
                vm.GameObjects = vm.UnfilteredGameObjects = await vm.RootTemplateHelper.LoadRelevent(combo.SelectedItem.ToString());
                listCountBlock.Text = $"{vm.GameObjects.Sum(x => x.Count) + vm.GameObjects.Count} Results";
                ToggleControls(true);
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as GameObjectViewModel;
            vm.GameObjects = vm.Filter(searchBox.Text ?? string.Empty);
            listCountBlock.Text = $"{vm.GameObjects.Sum(x => x.Count) + vm.GameObjects.Count} Results";
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Click(sender, e);
            }
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

        private void Collapsed(object sender, RoutedEventArgs e)
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
            if(gameObjects != null)
            {
                foreach (var gameObject in gameObjects)
                {
                    var infoContents = new StackPanel { Orientation = Orientation.Vertical };
                    infoContents.Children.Add(new TextBox { Text = $"Name: {gameObject.Name}", BorderThickness = new Thickness(0, 0, 0, 0), IsReadOnly = true });
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
        }

        /// <summary>
        /// Toggles controls on or off.
        /// </summary>
        /// <param name="enable">The sender to ignore.</param>
        private void ToggleControls(bool enable = false)
        {
            typeComboBox.IsEnabled = enable;
            search.IsEnabled = enable;
            searchBox.IsEnabled = enable;
        }

        /// <summary>
        /// Displays information for the selected game object.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ExploreMore_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var vm = DataContext as GameObjectViewModel;
            if(vm.DisabledButton != null)
                vm.DisabledButton.IsEnabled = true;
            vm.DisabledButton = button;
            button.IsEnabled = false;
            var MapKey = ((Button)sender).Uid;
            vm.Info = vm.RootTemplateHelper.FlatGameObjects.Single(go => go.MapKey == MapKey);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as GameObjectViewModel;
            vm.Clear();
        }

        private void TypeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            typeOptions.Collection = Services.RootTemplateHelper.GameObjectTypes;
            combo.SelectedIndex = 0;
        }
    }
}
