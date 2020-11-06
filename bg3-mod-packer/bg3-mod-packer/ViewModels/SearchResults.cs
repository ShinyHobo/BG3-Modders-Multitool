/// <summary>
/// The searcher view model.
/// </summary>
namespace bg3_mod_packer.ViewModels
{
    using bg3_mod_packer.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;

    /// <summary>
    /// The model for search results
    /// </summary>
    public class SearchResults : BaseViewModel
    {
        public SearchResults()
        {
            IndexHelper = new IndexHelper
            {
                DataContext = this
            };
            IsIndexing = false;
        }

        /// <summary>
        /// Gets the time that has passed since indexing began.
        /// </summary>
        /// <returns>The time taken.</returns>
        public TimeSpan GetTimeTaken()
        {
            return TimeSpan.FromTicks(DateTime.Now.Subtract(IndexStartTime).Ticks);
        }

        private ObservableCollection<SearchResult> _fileContents;

        public ObservableCollection<SearchResult> FileContents {
            get { return _fileContents; }
            set {
                _fileContents = value;
                OnNotifyPropertyChanged();
            }
        }

        #region Properties
        public IndexHelper IndexHelper { get; set; }

        private int _resultTotal;

        public int IndexFileTotal {
            get { return _resultTotal; }
            set {
                _resultTotal = value;
                OnNotifyPropertyChanged();
            }
        }

        private int _resultCount;

        public int IndexFileCount {
            get { return _resultCount; }
            set {
                _resultCount = value;
                var filesRemaining = IndexFileTotal - IndexFileCount;
                var timeTaken = GetTimeTaken();
                var timeRemaining = timeTaken.TotalMinutes / value * filesRemaining;
                if(timeRemaining < TimeSpan.MaxValue.TotalMinutes)
                {
                    IndexTimeRemaining = TimeSpan.FromMinutes(timeRemaining);
                }
                OnNotifyPropertyChanged();
            }
        }

        private ObservableCollection<SearchResult> _results;

        public ObservableCollection<SearchResult> Results {
            get { return _results; }
            set {
                _results = value;
                OnNotifyPropertyChanged();
            }
        }

        private DateTime _indexStartTime;

        public DateTime IndexStartTime {
            get { return _indexStartTime; }
            set {
                _indexStartTime = value;
                OnNotifyPropertyChanged();
            }
        }

        private TimeSpan _indexTimeRemaining;

        public TimeSpan IndexTimeRemaining {
            get { return _indexTimeRemaining; }
            set {
                _indexTimeRemaining = value;
                OnNotifyPropertyChanged();
            }
        }

        private Visibility _indexingVisibility;

        public Visibility IndexingVisibility {
            get { return _indexingVisibility; }
            set {
                _indexingVisibility = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _allowIndexing;

        public bool AllowIndexing {
            get { return _allowIndexing; }
            set {
                _allowIndexing = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _isIndexing;

        public bool IsIndexing {
            get { return _isIndexing; }
            set {
                _isIndexing = value;
                IndexingVisibility = value ? Visibility.Visible : Visibility.Hidden;
                AllowIndexing = !value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion
    }

    /// <summary>
    /// The model for a single search result.
    /// </summary>
    public class SearchResult : BaseViewModel
    {
        private string _path;

        public string Path {
            get { return _path; }
            set {
                _path = value;
                OnNotifyPropertyChanged();
            }
        }

        private int _key;

        public int Key {
            get { return _key; }
            set {
                _key = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _text;

        public string Text {
            get { return _text; }
            set {
                _text = value;
                OnNotifyPropertyChanged();
            }
        }
    }
}
