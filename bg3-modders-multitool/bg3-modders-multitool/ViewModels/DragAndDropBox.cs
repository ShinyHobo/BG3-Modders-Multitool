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

        public async Task ProcessClick(IDataObject data)
        {
            PackAllowed = false;
            _packAllowedDrop = false;
            await Services.DragAndDropHelper.ProcessDrop(data).ContinueWith(delegate
            {
                PackAllowed = true;
            });
        }

        public async Task ProcessDrop(IDataObject data)
        {
            PackAllowed = false;
            _packAllowedDrop = false;
            await Services.DragAndDropHelper.ProcessDrop(data).ContinueWith(delegate
            {
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

        private string _descriptionColor;
        private bool _packAllowed;
        private bool _packAllowedDrop;
        private string _packBoxColor;

        private string _packBoxInstructions;

        public string DescriptionColor
        {
            get { return _descriptionColor; }
            set
            {
                _descriptionColor = value;
                OnNotifyPropertyChanged();
            }
        }

        public bool PackAllowed
        {
            get { return _packAllowed; }
            set
            {
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
                PackBoxInstructions = value || _packAllowedDrop ? "Drop mod workspace folder here" : "Select divine.exe location";
                OnNotifyPropertyChanged();
            }
        }

        public string PackBoxColor
        {
            get { return _packBoxColor; }
            set
            {
                _packBoxColor = value;
                OnNotifyPropertyChanged();
            }
        }

        public string PackBoxInstructions
        {
            get { return _packBoxInstructions; }
            set
            {
                _packBoxInstructions = value;
                OnNotifyPropertyChanged();
            }
        }

        #endregion Properties
    }
}