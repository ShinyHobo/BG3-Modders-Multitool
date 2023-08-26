/// <summary>
/// The drag and drop box view model.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using System.Threading.Tasks;
    using System.Windows;

    public class DragAndDropBox : BaseViewModel
    {
        public DragAndDropBox()
        {
            PackAllowed = !string.IsNullOrEmpty(Properties.Settings.Default.divineExe);
            _packAllowedDrop = PackAllowed;
        }

        public async Task ProcessDrop(IDataObject data)
        {
            PackAllowed = false;
            _packAllowedDrop = false;
            await Services.DragAndDropHelper.ProcessDrop(data).ContinueWith(delegate {
                PackAllowed = true;
            });
        }

        internal void Darken()
        {
            PackBoxColor = PackAllowed ? "LightGreen" : "MidnightBlue";
            DescriptionColor = PackAllowed ? "Black" : "White";
        }

        internal void Lighten()
        {
            PackBoxColor = "LightBlue";
            DescriptionColor = "Black";
        }

        #region Properties
        private string _packBoxColor;

        public string PackBoxColor {
            get { return _packBoxColor; }
            set {
                _packBoxColor = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _descriptionColor;

        public string DescriptionColor {
            get { return _descriptionColor; }
            set {
                _descriptionColor = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _packAllowed;
        private bool _packAllowedDrop;

        public bool PackAllowed {
            get { return _packAllowed; }
            set {
                _packAllowed = value;
                if (value)
                {
                    Lighten();
                    _packAllowedDrop = value;
                }
                else
                {
                    Darken();
                }
                PackBoxInstructions = value || _packAllowedDrop ? Properties.Resources.DropModMessage : Properties.Resources.SelectDivineMessage;
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
        #endregion
    }
}
