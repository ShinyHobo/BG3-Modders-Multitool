/// <summary>
/// The drag and drop box view model.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;

    public class DragAndDropBox : BaseViewModel
    {
        public DragAndDropBox()
        {
            PackAllowed = true;
            _packAllowedDrop = PackAllowed;
            CanRebuild = Visibility.Collapsed;
            if(Directory.Exists(Properties.Settings.Default.rebuildLocation))
            {
                LastDirectory = Properties.Settings.Default.rebuildLocation;
            }
            else
            {
                Properties.Settings.Default.rebuildLocation = null;
                Properties.Settings.Default.Save();
            }
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

        private string _lastDirectory;
        public string LastDirectory {
            get { return _lastDirectory; }
            set {
                _lastDirectory = value;

                if(Directory.Exists(value) && value != Properties.Settings.Default.rebuildLocation)
                {
                    Properties.Settings.Default.rebuildLocation = value;
                    Properties.Settings.Default.Save();
                }

                CanRebuild = !string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Collapsed;
                OnNotifyPropertyChanged();
            }
        }

        private Visibility _canRebuild;
        public Visibility CanRebuild { 
            get { return _canRebuild; } 
            set {
                _canRebuild = value;
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

        private ulong _version;
        /// <summary>
        /// The mod version stored in the meta.lsx file as an Int64 value
        /// </summary>
        public ulong Version
        {
            get { return _version; }
            set {
                _version = value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion
    }
}
