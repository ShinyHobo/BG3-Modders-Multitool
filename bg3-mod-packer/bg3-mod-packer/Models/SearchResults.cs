namespace bg3_mod_packer.Models
{
    using bg3_mod_packer.Helpers;
    using bg3_mod_packer.ViewModels;
    using System.Collections.ObjectModel;

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
    }
}
