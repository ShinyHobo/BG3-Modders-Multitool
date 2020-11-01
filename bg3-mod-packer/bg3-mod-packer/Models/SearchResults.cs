namespace bg3_mod_packer.Models
{
    using bg3_mod_packer.Helpers;
    using bg3_mod_packer.ViewModels;
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
