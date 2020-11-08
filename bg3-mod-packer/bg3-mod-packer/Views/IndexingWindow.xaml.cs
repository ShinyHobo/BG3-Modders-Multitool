namespace bg3_mod_packer.Views
{
    using bg3_mod_packer.Services;
    using bg3_mod_packer.ViewModels;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
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
                var vm = DataContext as SearchResults;
                vm.SelectedPath = string.Empty;
                vm.FileContents = new ObservableCollection<SearchResult>();
                vm.Results = new ObservableCollection<SearchResult>();
                foreach (string result in await vm.IndexHelper.SearchFiles(search.Text))
                {
                    vm.Results.Add(new SearchResult { Path = result.Replace(@"\\?\", string.Empty).Replace(@"\\", @"\").Replace($"{Directory.GetCurrentDirectory()}\\UnpackedData\\",string.Empty) });
                }
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
                    vm.FileContents = new ObservableCollection<SearchResult>();
                    vm.SelectedPath = ((TextBlock)pathButton.Content).Text;
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
            var newFile = FileHelper.ConvertToLsx(vm.SelectedPath);
            FileHelper.OpenFile(newFile);
            convertAndOpenButton.IsEnabled = true;
        }
    }
}
