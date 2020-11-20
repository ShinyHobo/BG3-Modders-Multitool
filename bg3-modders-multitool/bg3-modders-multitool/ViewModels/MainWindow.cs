/// <summary>
/// The model for the main window instance.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using bg3_modders_multitool.Services;
    using System;
    using System.Windows;

    public class MainWindow : BaseViewModel
    {
        public MainWindow()
        {
            DivineLocation = Properties.Settings.Default.divineExe;
            Bg3ExeLocation = Properties.Settings.Default.bg3Exe;
            Unpacker = new PakUnpackHelper();
            LaunchGameAllowed = !string.IsNullOrEmpty(Bg3ExeLocation);
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
            var guidText = isHandle ? HandleText : GuidText;
            var type = isHandle ? "TranslatedString handle" : "v4 UUID";
            if (guidText != null)
            {
                Clipboard.SetText(guidText);
                ConsoleOutput += $"{type} [{guidText}] copied to clipboard!\n";
            }
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
