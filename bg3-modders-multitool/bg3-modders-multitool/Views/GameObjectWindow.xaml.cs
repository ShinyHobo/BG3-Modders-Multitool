/// <summary>
/// The game object exploration window.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using Assimp;
    using bg3_modders_multitool.Services;
    using bg3_modders_multitool.ViewModels;
    using HelixToolkit.Wpf;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using Material = System.Windows.Media.Media3D.Material;

    /// <summary>
    /// Interaction logic for GameObjectWindow.xaml
    /// </summary>
    public partial class GameObjectWindow : Window
    {
        public GameObjectWindow()
        {
            InitializeComponent();
            DataContext = new GameObjectViewModel();
            // convert GR2 file to dae with divine.exe
            var filename = @"J:\BG3\bg3-modders-multitool\bg3-modders-multitool\bg3-modders-multitool\bin\x64\Debug\UnpackedData\Models\Public\Shared\Assets\Characters\_Models\_Creatures\Dragon_Red\Dragon_Red_A";
            // J:\BG3\6-1603420546-7833522.png
            if (!File.Exists($"{filename}.obj"))
            {
                var dae = $"{filename}.dae";
                // read stream in
                // keep track of vertices id
                // when vertext input is found, replace source with vertices id
                // read to end
                AssimpContext converter = new AssimpContext();
                var imported = converter.ImportFile(dae);
                converter.ExportFile(imported, $"{filename}.obj", "obj");
                // update mtl file with correct values ie texture (requires conversion of dds to png?)
            }
            //var importer = new ModelImporter();
            //var bler = importer.Load($"{filename}.obj");
            var objModel = new ObjReader().Read($"{filename}.obj");
            model.Content = objModel;
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
                vm.GameObjects = vm.UnfilteredGameObjects = await vm.RootTemplateHelper.LoadRelevent((Enums.GameObjectType)combo.SelectedItem);
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
            var MapKey = button.Uid;
            if (MapKey != vm.SelectedKey)
            {
                var disabledButton = GeneralHelper.FindUid(treeView, vm.SelectedKey);
                if (disabledButton != null)
                    disabledButton.IsEnabled = true;
                vm.Info = vm.FindGameObject(MapKey);
                vm.SelectedKey = MapKey;
                button.IsEnabled = false;
            }
        }

        private void TypeComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            var vm = DataContext as GameObjectViewModel;
            typeOptions.Collection = vm.RootTemplateHelper.GameObjectTypes;
            combo.SelectedIndex = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as GameObjectViewModel;
            vm.Clear();
        }

        /// <summary>
        /// Disables button when it is loaded by the VirtualizingStackPanel.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void ItemSelectionButton_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as GameObjectViewModel;
            var button = (Button)sender;
            var MapKey = button.Uid;
            if(MapKey == vm.SelectedKey)
            {
                button.IsEnabled = false;
            }
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

        #region PropertyGrid Events
        /// <summary>
        /// Sets up loading properties to modify editors.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void PropertyGrid_SelectedObjectChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var grid = sender as Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid;
            var properties = grid.Properties.OfType<Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem>();
            foreach(var prop in properties)
            {
                prop.Loaded += Prop_Loaded;
            }
        }

        /// <summary>
        /// Sets up PropertyGrid displays.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void Prop_Loaded(object sender, RoutedEventArgs e)
        {
            var source = e.OriginalSource as Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem;
            if (source.Editor is Xceed.Wpf.Toolkit.PropertyGrid.Editors.PropertyGridEditorComboBox)
            {
                var editor = source.Editor as Xceed.Wpf.Toolkit.PropertyGrid.Editors.PropertyGridEditorComboBox;
                editor.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            }
            else if(source.Editor is Xceed.Wpf.Toolkit.PropertyGrid.Editors.PropertyGridEditorPrimitiveTypeCollectionControl)
            {
                var editor = source.Editor as Xceed.Wpf.Toolkit.PropertyGrid.Editors.PropertyGridEditorPrimitiveTypeCollectionControl;
                var text = string.Empty;
                if (editor.ItemsSource != null)
                {
                    foreach (var item in editor.ItemsSource)
                    {
                        text += System.Enum.GetName(item.GetType(), item) + ";";
                    }
                }
                editor.Content = text;
            }
        }

        /// <summary>
        /// Disables dropdown options at runtime.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        private void ItemContainerGenerator_StatusChanged(object sender, System.EventArgs e)
        {
            var itemContainerGenerator = sender as ItemContainerGenerator;
            if(itemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                foreach (var item in itemContainerGenerator.Items)
                {
                    var container = itemContainerGenerator.ContainerFromItem(item) as ComboBoxItem;
                    container.IsEnabled = false;
                }

            }
        }
        #endregion
    }
}
