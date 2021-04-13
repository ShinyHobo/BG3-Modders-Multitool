/// <summary>
/// The index search window.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using bg3_modders_multitool.Services;
    using bg3_modders_multitool.ViewModels;
    using HelixToolkit.Wpf.SharpDX;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for IndexingWindow.xaml
    /// </summary>
    public partial class IndexingWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private bool isMouseOver = false;
        private string hoverFile;
        private Button pathButton;

        public IndexingWindow()
        {
            InitializeComponent();
            DataContext = new SearchResults();
            ((SearchResults)DataContext).IndexHelper.DataContext = (SearchResults)DataContext;
            timer.Interval = TimeSpan.FromMilliseconds(400);
            timer.Tick += Timer_Tick;
        }

        private async void SearchFiles_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(search.Text))
            {
                searchFilesButton.IsEnabled = false;
                var vm = DataContext as SearchResults;
                vm.SelectedPath = string.Empty;
                vm.FileContents = new ObservableCollection<SearchResult>();
                vm.Results = new ObservableCollection<SearchResult>();
                foreach (string result in await vm.IndexHelper.SearchFiles(search.Text))
                {
                    vm.Results.Add(new SearchResult { Path = result.Replace(@"\\?\", string.Empty).Replace(@"\\", @"\").Replace($"{Directory.GetCurrentDirectory()}\\UnpackedData\\",string.Empty) });
                }
                searchFilesButton.IsEnabled = true;
            }
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchFiles_Click(sender, e);
            }
        }

        private void Path_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileHelper.OpenFile(((TextBlock)((Button)sender).Content).Text);
        }

        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
            pathButton = (Button)sender;
            hoverFile = FileHelper.GetPath(((TextBlock)pathButton.Content).Text);
            timer.Start();
        }

        private void Path_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
            timer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isMouseOver)
            {
                var vm = DataContext as SearchResults;
                if (string.IsNullOrEmpty(vm.SelectedPath)||!hoverFile.Contains(vm.SelectedPath))
                {
                    vm.ModelLoading = Visibility.Hidden;
                    vm.ModelVisible = Visibility.Hidden;
                    vm.FileContents = new ObservableCollection<SearchResult>();
                    vm.SelectedPath = ((TextBlock)pathButton.Content).Text;
                    if (Path.GetExtension(vm.SelectedPath).Contains(".GR2") || Path.GetExtension(vm.SelectedPath).Contains(".gr2"))
                    {
                        vm.ModelLoading = Visibility.Visible;
                        var modelsToRemove = viewport.Items.Where(i => i as MeshGeometryModel3D != null).ToList();
                        foreach (var model in modelsToRemove)
                        {
                            viewport.Items.Remove(model);
                        }
                        var mesh = RenderedModelHelper.GetMesh(Path.ChangeExtension(FileHelper.GetPath(vm.SelectedPath), null));
                        if(mesh != null && mesh.Any())
                        {
                            var lod = mesh.First().Value;
                            foreach (var model in lod)
                            {
                                var meshGeometry = new MeshGeometryModel3D() { Geometry = model, Material = vm.Material, CullMode = SharpDX.Direct3D11.CullMode.Back, Transform = vm.Transform };
                                viewport.Items.Add(meshGeometry);
                            }
                            vm.ModelVisible = Visibility.Visible;
                        }
                        vm.ModelLoading = Visibility.Hidden;
                    }
                    foreach (var content in vm.IndexHelper.GetFileContents(hoverFile))
                    {
                        vm.FileContents.Add(new SearchResult { Key = content.Key, Text = content.Value.Trim() });
                    }
                    convertAndOpenButton.IsEnabled = true;
                    convertAndOpenButton.Content = FileHelper.CanConvertToLsx(vm.SelectedPath) ? "Convert & Open" : "Open";
                }
            }
            timer.Stop();
        }

        private void ConvertAndOpenButton_Click(object sender, RoutedEventArgs e)
        {
            convertAndOpenButton.IsEnabled = false;
            var vm = DataContext as SearchResults;
            var newFile = FileHelper.Convert(vm.SelectedPath,"lsx");
            FileHelper.OpenFile(newFile);
            convertAndOpenButton.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as SearchResults;
            vm.Clear();
        }
    }
}
