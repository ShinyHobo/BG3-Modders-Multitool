/// <summary>
/// The drag and drop box view model.
/// </summary>
namespace bg3_mod_packer.ViewModels
{
    public class DragAndDropBox : BaseViewModel
    {
        public DragAndDropBox()
        {
            PackAllowed = !string.IsNullOrEmpty(Properties.Settings.Default.divineExe);
        }

        private string _packBoxColor;

        public string PackBoxColor {
            get { return _packBoxColor; }
            set {
                _packBoxColor = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _packAllowed;

        public bool PackAllowed {
            get { return _packAllowed; }
            set {
                _packAllowed = value;
                PackBoxColor = value ? "LightBlue" : "#FF265868";
                PackBoxInstructions = value ? "Drop mod workspace folder here" : "Select divine.exe location";
                OnNotifyPropertyChanged();
            }
        }

        private string _packBoxInstructions;

        public string PackBoxInstructions {
            get { return _packBoxInstructions; }
            set {
                _packBoxInstructions = value;
                OnNotifyPropertyChanged();
            }
        }
    }
}
