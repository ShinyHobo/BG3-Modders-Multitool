/// <summary>
/// The model for the main window instance.
/// </summary>
namespace bg3_mod_packer.ViewModels
{
    using bg3_mod_packer.Services;

    public class MainWindow : BaseViewModel
    {
        public MainWindow()
        {
            DivineLocation = Properties.Settings.Default.divineExe;
            Bg3ExeLocation = Properties.Settings.Default.bg3Exe;
        }

        public PakUnpackHelper UnpackingProcess { get; internal set; }

        private string _consoleOutput;

        public string ConsoleOutput {
            get { return _consoleOutput; }
            set {
                _consoleOutput = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _unpackAllowed;

        public bool UnpackAllowed {
            get { return _unpackAllowed; }
            set {
                _unpackAllowed = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _divineLocation;

        public string DivineLocation {
            get { return _divineLocation; }
            set {
                _divineLocation = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _bg3exeLocation;

        public string Bg3ExeLocation {
            get { return _bg3exeLocation; }
            set {
                _bg3exeLocation = value;
                UnpackAllowed = !string.IsNullOrEmpty(value);
                OnNotifyPropertyChanged();
            }
        }

        private int _indexFileCount;

        public int IndexFileCount {
            get { return _indexFileCount; }
            set {
                _indexFileCount = value;
                OnNotifyPropertyChanged();
            }
        }

        private int _indexFileTotal;

        public int IndexFileTotal {
            get { return _indexFileTotal; }
            set {
                _indexFileTotal = value;
                OnNotifyPropertyChanged();
            }
        }
    }
}
