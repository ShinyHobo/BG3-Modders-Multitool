/// <summary>
/// The game object exploration window.
/// </summary>
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

        #region Events
        /// <summary>
        /// Loads in relevant game objects by type.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private async void Type_Change(object sender, RoutedEventArgs e)
        {
            var combo = (ComboBox)sender;
            if(combo.SelectedIndex != 0)
            {
                searchBox.Text = string.Empty;
                ToggleControls();
                var vm = DataContext as GameObjectViewModel;
                vm.GameObjects = vm.UnfilteredGameObjects = await vm.RootTemplateHelper.LoadRelevent(combo.SelectedItem.ToString());
                listCountBlock.Text = $"{vm.GameObjects.Sum(x => x.Count())} Results";
                ToggleControls(true);
            }
        }

        /// <summary>
        /// Filters the loaded game objects by a search parameter.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as GameObjectViewModel;
            vm.GameObjects = vm.Filter(searchBox.Text ?? string.Empty);
            listCountBlock.Text = $"{vm.GameObjects.Sum(x => x.Count())} Results";
        }

        /// <summary>
        /// Activates the search filter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_Click(sender, e);
            }
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
            if (vm.DisabledButton != null)
                vm.DisabledButton.IsEnabled = true;
            vm.DisabledButton = button;
            button.IsEnabled = false;
            var MapKey = ((Button)sender).Uid;
            vm.Info = vm.RootTemplateHelper.FlatGameObjects.Single(go => go.MapKey == MapKey);
        }

        private void TypeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            typeOptions.Collection = Services.RootTemplateHelper.GameObjectTypes;
            combo.SelectedIndex = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as GameObjectViewModel;
            vm.Clear();
        }
        #endregion

        #region Helpers
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
        #endregion
    }
}
