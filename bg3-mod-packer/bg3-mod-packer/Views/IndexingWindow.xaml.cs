namespace bg3_mod_packer.Views
{
    using bg3_mod_packer.Helpers;
    using bg3_mod_packer.Models;
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

        private async void IndexFiles_Click(object sender, RoutedEventArgs e)
        {
            await ((SearchResults)DataContext).IndexHelper.Index();
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
            IndexHelper.OpenFile(((TextBlock)((Button)sender).Content).Text);
        }

        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
            pathButton = (Button)sender;
            hoverFile = IndexHelper.GetPath(((TextBlock)pathButton.Content).Text);
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
                var contents = new ObservableCollection<SearchToolTip>();
                foreach(var content in ((SearchResults)DataContext).IndexHelper.GetFileContents(hoverFile))
                {
                    contents.Add(new SearchToolTip { Key = content.Key, Text = content.Value.Trim()});
                }
                ((SearchResult)pathButton.DataContext).FileContents = contents;
            }
            timer.Stop();
        }
    }
}
