/// <summary>
/// The index search window.
/// </summary>
namespace bg3_modders_multitool.Views
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Services;
    using bg3_modders_multitool.ViewModels;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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
        private SearchResults SearchResults = new SearchResults();

        public IndexingWindow()
        {
            InitializeComponent();
            DataContext = SearchResults;

            ((SearchResults)DataContext).IndexHelper.DataContext = (SearchResults)DataContext;
            ((SearchResults)DataContext).ViewPort = viewport;
            timer.Interval = TimeSpan.FromMilliseconds(400);
            timer.Tick += Timer_Tick;

            // TODO - get full list of file types from somewhere
            fileTypeFilter.ItemsSource = FileHelper.FileTypes;
            fileTypeFilter.IsSelectAllActive = true;
            fileTypeFilter.SelectAll();

            ((SearchResults)DataContext).LeadingWildcardDisabled = false;
        }

        private async void SearchFiles_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(search.Text) && fileTypeFilter.SelectedItems.Count > 0)
            {
                searchFilesButton.IsEnabled = false;
                leadingWildcardDisabledCheckbox.IsEnabled = false;
                fileTypeFilter.IsEnabled = false;
                search.IsEnabled = false;
                convertAndOpenButton.IsEnabled = false;
                SearchResults.SelectedPath = string.Empty;
                SearchResults.FileContents = new ObservableCollection<SearchResult>();
                SearchResults.Results = new ObservableCollection<SearchResult>();
                var matches = await SearchResults.IndexHelper.SearchFiles(search.Text, true, fileTypeFilter.SelectedItems, !SearchResults.LeadingWildcardDisabled);
                SearchResults.FullResultList = matches.Matches.ToList();
                SearchResults.FullResultList.AddRange(matches.FilteredMatches.ToList());
                SearchResults.FullResultList.Sort();
                foreach (string result in matches.Matches)
                {
                    SearchResults.Results.Add(new SearchResult { Path = result });
                }
                SearchResults.Results = new ObservableCollection<SearchResult>(SearchResults.Results.OrderBy(x => x.Path));
                searchFilesButton.IsEnabled = true;
                fileTypeFilter.IsEnabled = true;
                search.IsEnabled = true;
                convertAndOpenButton.IsEnabled = true;
                leadingWildcardDisabledCheckbox.IsEnabled = true;
                search.Focus();
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
                if (string.IsNullOrEmpty(SearchResults.SelectedPath)|| hoverFile != FileHelper.GetPath(SearchResults.SelectedPath))
                {
                    SearchResults.FileContents = new ObservableCollection<SearchResult>();
                    SearchResults.SelectedPath = ((TextBlock)pathButton.Content).Text;
                    var isGr2 = SearchResults.RenderModel();
                    foreach (var content in SearchResults.IndexHelper.GetFileContents(hoverFile))
                    {
                        SearchResults.FileContents.Add(new SearchResult { Key = content.Key + 1, Text = content.Value.Trim() });
                    }
                    convertAndOpenButton.IsEnabled = true;
                    if(isGr2)
                    {
                        convertAndOpenButton.Content = Properties.Resources.OpenDaeButton;
                    }
                    else if(FileHelper.CanConvertToLsx(SearchResults.SelectedPath)|| SearchResults.SelectedPath.EndsWith(".loca"))
                    {
                        convertAndOpenButton.Content = Properties.Resources.ConvertAndOpenButton;
                    }
                    else
                    {
                        convertAndOpenButton.Content = Properties.Resources.OpenButton;
                    }
                }
            }
            timer.Stop();
        }

        private void ConvertAndOpenButton_Click(object sender, RoutedEventArgs e)
        {
            convertAndOpenButton.IsEnabled = false;
            var ext = Path.GetExtension(SearchResults.SelectedPath);
            var selectedPath = FileHelper.GetPath(SearchResults.SelectedPath);

            PakReaderHelper.OpenPakFile(SearchResults.SelectedPath);
            
            if(ext == ".loca")
            {
                var newFile = FileHelper.Convert(selectedPath, "xml");
                FileHelper.OpenFile(newFile);
            }
            else
            {
                var newFile = FileHelper.Convert(selectedPath, "lsx");
                FileHelper.OpenFile(newFile);
            }
            convertAndOpenButton.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as SearchResults;
            vm.Clear();
        }

        private CancellationTokenSource UpdateFilterCancellation { get; set; }

        private void fileTypeFilter_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            if (UpdateFilterCancellation != null)
            {
                UpdateFilterCancellation.Cancel();
            }
            UpdateFilterCancellation = new CancellationTokenSource();
            CancellationToken ct = UpdateFilterCancellation.Token;

            Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (SearchResults.FullResultList != null)
                    {

                        SearchResults.FileContents = new ObservableCollection<SearchResult>();
                        SearchResults.Results = new ObservableCollection<SearchResult>();
                        foreach (string result in SearchResults.FullResultList)
                        {
                            var ext = Path.GetExtension(result).ToLower();
                            ext = string.IsNullOrEmpty(ext) ? Properties.Resources.Extensionless : ext;
                            if (fileTypeFilter.SelectedItems != null && fileTypeFilter.SelectedItems.Contains(ext))
                            {
                                SearchResults.Results.Add(new SearchResult { Path = result });
                            }
                        }
                        SearchResults.Results = new ObservableCollection<SearchResult>(SearchResults.Results.OrderBy(x => x.Path));
                    }
                });
            }, ct);
        }

        private void lineNumberButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var content = btn.Content as TextBlock;
            var line = int.Parse(content.Text.Split(':').First());
            var selectedPath = FileHelper.GetPath(SearchResults.SelectedPath);
            PakReaderHelper.OpenPakFile(SearchResults.SelectedPath);

            var ext = Path.GetExtension(SearchResults.SelectedPath);
            if (ext == ".loca")
            {
                var newFile = FileHelper.Convert(selectedPath, "xml");
                FileHelper.OpenFile(newFile, line);
            }
            else
            {
                var newFile = FileHelper.Convert(selectedPath, "lsx");
                FileHelper.OpenFile(newFile, line);
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchResults.SelectedPath))
            {
                var newFile = PakReaderHelper.OpenPakFile(SearchResults.SelectedPath);
                newFile = newFile ?? FileHelper.GetPath(SearchResults.SelectedPath);
                if(File.Exists(newFile))
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,{newFile}");
            }
        }

        private void ExtractFile_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(SearchResults.SelectedPath))
            {
                PakReaderHelper.OpenPakFile(SearchResults.SelectedPath);
                GeneralHelper.WriteToConsole(Properties.Resources.ResourceExtracted, SearchResults.SelectedPath);
            }
        }
    }
}
