namespace bg3_mod_packer.Models
{
    using bg3_mod_packer.Helpers;
    using bg3_mod_packer.ViewModels;
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// The model for search results
    /// </summary>
    public class SearchResults : BaseViewModel
    {
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
                var timeTaken = TimeSpan.FromTicks(DateTime.Now.Subtract(IndexStartTime).Ticks);
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

        public IndexHelper IndexHelper = new IndexHelper();

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

        private ObservableCollection<SearchToolTip> _fileContents;

        public ObservableCollection<SearchToolTip> FileContents {
            get { return _fileContents; }
            set {
                _fileContents = value;
                OnNotifyPropertyChanged();
            }
        }
    }

    /// <summary>
    /// The model for search result tooltips.
    /// </summary>
    public class SearchToolTip : BaseViewModel
    {
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
