/// <summary>
/// The model for the main window instance.
/// </summary>
namespace bg3_mod_packer.Models
{
    using bg3_mod_packer.Helpers;

    public class MainWindow : ViewModels.BaseViewModel
    {
        public PakUnpackHelper UnpackingProcess { get; internal set; }

        private string _consoleOutput;

        public string ConsoleOutput {
            get { return _consoleOutput; }
            set {
                _consoleOutput = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _divineLocation = Properties.Settings.Default.divineExe;

        public string DivineLocation {
            get { return _divineLocation; }
            set {
                _divineLocation = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _bg3exeLocation = Properties.Settings.Default.bg3Exe;

        public string Bg3ExeLocation {
            get { return _bg3exeLocation; }
            set {
                _divineLocation = value;
                OnNotifyPropertyChanged();
            }
        }
    }
}
