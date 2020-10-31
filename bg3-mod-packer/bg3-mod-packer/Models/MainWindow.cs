/// <summary>
/// The model for the main window instance.
/// </summary>
namespace bg3_mod_packer.Models
{
    using bg3_mod_packer.Helpers;

    public class MainWindow : ViewModels.BaseViewModel
    {
        private string _consoleOutput;

        public PakUnpackHelper UnpackingProcess { get; internal set; }

        public string ConsoleOutput {
            get { return _consoleOutput; }
            set {
                _consoleOutput = value;
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
