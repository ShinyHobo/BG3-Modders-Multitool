namespace bg3_mod_packer.Views
{
    using bg3_mod_packer.Helpers;
    using bg3_mod_packer.Models;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows;

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

        private void SearchFiles_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(search.Text))
            {
                var results = new ObservableCollection<SearchResult>();
                foreach (string result in ((SearchResults)DataContext).IndexHelper.SearchFiles(search.Text))
                {
                    results.Add(new SearchResult { Path = result.Replace(@"\\", @"\") });
                }
                ((SearchResults)DataContext).Results = results;
            }
        }
    }
}
