/// <summary>
/// The model for the main window instance.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using bg3_modders_multitool.Services;
    using System;
    using System.IO;
    using System.Windows;

    public class MainWindow : BaseViewModel
    {
        public MainWindow()
        {
            DivineLocation = Properties.Settings.Default.divineExe;
            Bg3ExeLocation = Properties.Settings.Default.bg3Exe;
            Unpacker = new PakUnpackHelper();
            LaunchGameAllowed = !string.IsNullOrEmpty(Bg3ExeLocation);
            QuickLaunch = Properties.Settings.Default.quickLaunch;

            ModsFolderLoaded = !string.IsNullOrEmpty(PathHelper.ModsFolderPath) && Directory.Exists(PathHelper.ModsFolderPath);
            ProfilesFolderLoaded = !string.IsNullOrEmpty(PathHelper.PlayerProfilesFolderPath) && Directory.Exists(PathHelper.PlayerProfilesFolderPath);
        }

        #region File Selection Methods
        /// <summary>
        /// Sets file location in application settings.
        /// </summary>
        /// <param name="property">The property to update.</param>
        /// <param name="title">The title to give the file selection window.</param>
        /// <returns>Returns the file location.</returns>
        public string FileLocationDialog(string property, string title)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Title = title
            };
            var result = fileDialog.ShowDialog();
            var file = (string)Properties.Settings.Default[property];
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    file = fileDialog.FileName;
                    Properties.Settings.Default[property] = file;
                    Properties.Settings.Default.Save();
                    break;
            }
            
            return file;
        }
        #endregion

        #region UUID Generation Methods
        /// <summary>
        /// Generates a guid for copying.
        /// </summary>
        /// <param name="isHandle">Whether the guid is a TranslatedString handle.</param>
        public void GenerateGuid(bool isHandle)
        {
            var guid = Guid.NewGuid();
            GuidText = isHandle ? $"h{guid}".Replace('-', 'g') : guid.ToString();
        }

        /// <summary>
        /// Copies the guid to the clipboard.
        /// </summary>
        /// <param name="isHandle">Whether the guid is a TranslatedString handle.</param>
        public void CopyGuid(bool isHandle)
        {
            var type = isHandle ? "TranslatedString handle" : "v4 UUID";
            if (GuidText != null)
            {
                Clipboard.SetText(GuidText);
                ConsoleOutput += $"{type} [{GuidText}] copied to clipboard!\n";
            }
        }
        #endregion

        #region Configuration Methods
        /// <summary>
        /// Toggles the quick launch features.
        /// </summary>
        /// <param name="setting">Whether or not quick launch options should be enabled.</param>
        public void ToggleQuickLaunch(bool setting)
        {
            GeneralHelper.ToggleQuickLaunch(setting);
        }
        #endregion

        #region Properties
        public PakUnpackHelper Unpacker { get; set; }

        public DragAndDropBox DragAndDropBox { get; set; }

        public SearchResults SearchResults { get; set; }

        private string _consoleOutput;

        public string ConsoleOutput {
            get { return _consoleOutput; }
            set {
                _consoleOutput = value;
                OnNotifyPropertyChanged();
            }
        }

        #region File Selection Properties
        private bool _unpackAllowed;

        public bool UnpackAllowed {
            get { return _unpackAllowed; }
            set {
                _unpackAllowed = value;
                NotDecompressing = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _divineLocation;

        public string DivineLocation {
            get { return _divineLocation; }
            set {
                _divineLocation = value;
                UnpackAllowed = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(Bg3ExeLocation);
                if (DragAndDropBox != null)
                {
                    DragAndDropBox.PackAllowed = !string.IsNullOrEmpty(value);
                }
                OnNotifyPropertyChanged();
            }
        }

        private string _bg3exeLocation;

        public string Bg3ExeLocation {
            get { return _bg3exeLocation; }
            set {
                _bg3exeLocation = value;
                UnpackAllowed = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(DivineLocation);
                LaunchGameAllowed = !string.IsNullOrEmpty(value);
                ConfigNeeded = UnpackAllowed && LaunchGameAllowed ? Visibility.Hidden : Visibility.Visible;
                OnNotifyPropertyChanged();
            }
        }

        private bool _configOpen = false;

        public bool ConfigOpen {
            get { return _configOpen; }
            set {
                _configOpen = value;
                OnNotifyPropertyChanged();
            }
        }

        private Visibility _configNeeded = Visibility.Hidden;

        public Visibility ConfigNeeded {
            get { return _configNeeded; }
            set {
                _configNeeded = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _launchGameAllowed;

        public bool LaunchGameAllowed {
            get { return _launchGameAllowed; }
            set {
                _launchGameAllowed = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _notDecompressing;

        public bool NotDecompressing {
            get { return _notDecompressing; }
            set {
                _notDecompressing = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _quickLaunch;

        public bool QuickLaunch {
            get { return _quickLaunch; }
            set {
                _quickLaunch = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _modsFolderLoaded;

        public bool ModsFolderLoaded
        {
            get { return _modsFolderLoaded; }
            set
            {
                _modsFolderLoaded = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _profilesFolderLoaded;

        public bool ProfilesFolderLoaded
        {
            get { return _profilesFolderLoaded; }
            set
            {
                _profilesFolderLoaded = value;
                OnNotifyPropertyChanged();
            }
        }

        #endregion

        #region UUID Generation Properties
        private string _guidText;

        public string GuidText {
            get { return _guidText;  }
            set {
                _guidText = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _handleText;

        public string HandleText {
            get { return _handleText; }
            set {
                _handleText = value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion
        #endregion
    }
}
