namespace bg3_mod_packer.Views
{
    using bg3_mod_packer.Helpers;
    using bg3_mod_packer.Models;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for IndexingWindow.xaml
    /// </summary>
    public partial class IndexingWindow : Window
    {
        public IndexingWindow()
        {
            InitializeComponent();
            DataContext = new SearchResults();
            ((SearchResults)DataContext).IndexHelper.DataContext = (SearchResults)DataContext;
        }

        private async void IndexFiles_Click(object sender, RoutedEventArgs e)
        {
            var fileList = IndexHelper.DirectorySearch("UnpackedData");
            var fileExtensions = IndexHelper.GetFileExtensions(fileList);
            await ((SearchResults)DataContext).IndexHelper.Index(fileList);
        }

        private async void SearchFiles_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(search.Text))
            {
                var results = new ObservableCollection<SearchResult>();
                foreach (string result in await ((SearchResults)DataContext).IndexHelper.SearchFiles(search.Text))
                {
                    results.Add(new SearchResult { Path = result.Replace(@"\\", @"\").Replace($"{Directory.GetCurrentDirectory()}\\UnpackedData\\",string.Empty) });
                }
                ((SearchResults)DataContext).Results = results;
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
            var path = ((TextBlock)((Button)sender).Content).Text;
            System.Diagnostics.Process fileopener = new System.Diagnostics.Process();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = $"{Directory.GetCurrentDirectory()}\\UnpackedData\\{path}";
            fileopener.Start();
        }
    }
}
