/// <summary>
/// The model for the main window instance.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using bg3_modders_multitool.Properties;
    using bg3_modders_multitool.Services;
    using bg3_modders_multitool.Views;
    using MdXaml;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Documents;

    public class MainWindow : BaseViewModel
    {
        public MainWindow()
        {
            DivineLocation = Properties.Settings.Default.divineExe;
            Bg3ExeLocation = Properties.Settings.Default.bg3Exe;
            GameDocumentsLocation = Settings.Default.gameDocumentsPath;
            Unpacker = new PakUnpackHelper();
            LaunchGameAllowed = !string.IsNullOrEmpty(Bg3ExeLocation);
            QuickLaunch = Properties.Settings.Default.quickLaunch;
            ThreadsUnlocked = Properties.Settings.Default.unlockThreads;
            AutoUpdater = new AutoUpdaterService();
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

        /// <summary>
        /// Sets folder location in application settings.
        /// </summary>
        /// <param name="property">The property to update.</param>
        /// <param name="title">The title to give the folder selection window.</param>
        /// <returns>Returns the folder location.</returns>
        public string FolderLocationDialog(string property, string title)
        {
            var folder = (string)Settings.Default[property];
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = title,
            };

            var result = folderDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                { 
                    folder = folderDialog.SelectedPath;
                    Settings.Default[property] = folder;
                    Settings.Default.Save();
                    break;
                }
            }

            return folder;
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
            var type = isHandle ? Properties.Resources.TranslatedStringHandleLabel : Properties.Resources.v4UUIDLabel;
            if (GuidText != null)
            {
                // https://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid/17678542#17678542
                // https://stackoverflow.com/questions/930219/how-to-handle-a-blocked-clipboard-and-other-oddities
                try
                {
                    System.Windows.Forms.Clipboard.SetDataObject(GuidText, false, 10, 10);
                    GeneralHelper.WriteToConsole(Properties.Resources.GUIDCopied, type, GuidText);
                } 
                catch (Exception ex)
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.GUIDCopyFailed, GeneralHelper.ProcessHoldingClipboard().ProcessName, ex.Message, ex.StackTrace);
                }
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

        public AutoUpdaterService AutoUpdater { get; set; }

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
                ConfigNeeded = ValidateConfigNeeded();
                OnNotifyPropertyChanged();
            }
        }

        private string _gameDocumentsLocation;

        public string GameDocumentsLocation
        {
            get { return _gameDocumentsLocation; }
            set
            {
                _gameDocumentsLocation = value;

                // Validate the mods folder path.
                ModsFolderLoaded = Directory.Exists(PathHelper.ModsFolderPath);
                if(!ModsFolderLoaded)
                    GeneralHelper.WriteToConsole(Properties.Resources.UnableToFindModsFolder, PathHelper.ModsFolderPath);

                // Validate the player profiles folder path.
                ProfilesFolderLoaded = Directory.Exists(PathHelper.PlayerProfilesFolderPath);
                if (!ProfilesFolderLoaded)
                    GeneralHelper.WriteToConsole(Properties.Resources.UnableToFindPlayerProfilesFolder, PathHelper.PlayerProfilesFolderPath);

                ConfigNeeded = ValidateConfigNeeded();
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

        private bool _threadsUnlocked;

        public bool ThreadsUnlocked
        {
            get { return _threadsUnlocked; }
            set
            {
                _threadsUnlocked = value;
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

        #region Auxiliary Methods

        /// <summary>
        /// Appends a message as a new line to the ConsoleOutput string.
        /// </summary>
        /// <param name="message">The message to be appended.</param>
        public void WriteToConsole(string message)
        {
            // Add a timestamp to the message if requested.
            if (Settings.Default.enableConsoleTimestamp)
                message = $"[{DateTime.Now.ToLocalTime()}] {message}";

            // Add a newline to the message if it doesn't already have one.
            if(!message.EndsWith("\n"))
                message += "\n";

            ConsoleOutput += message;
        }

        /// <summary>
        /// Determines the value of <see cref="Visibility"/> which should have the ConfigNeeded Label
        /// based on whether current configuration is ok: all paths must be specified and valid.
        /// </summary>
        /// <returns></returns>
        private Visibility ValidateConfigNeeded()
        {
            return UnpackAllowed && LaunchGameAllowed && Directory.Exists(PathHelper.ModsFolderPath) && Directory.Exists(PathHelper.PlayerProfilesFolderPath) 
                ? Visibility.Hidden 
                : Visibility.Visible;
        }
        #endregion

        #region Language Selection
        public string SelectedLanguage { get; set; }

        /// <summary>
        /// The list of available languages and their I18N designations
        /// </summary>
        public static List<Language> AvailableLanguages = new List<Language>
            {
                new Language(Properties.Resources.LangEnglish, "en-US", "English\\Localization\\English\\english.loca"),
                new Language(Properties.Resources.LangChinese, "zh-CN", "Chinese\\Localization\\Chinese\\chinese.loca")
            };

        /// <summary>
        /// Reloads the application with the selected language code
        /// </summary>
        /// <param name="language"></param>
        public void ReloadLanguage(string language)
        {
            var selectedLanguage = GetSelectedLanguage().Code;
            if (selectedLanguage != language)
            {
                SelectedLanguage = language;
                App.Current.MainWindow.Close();
            }
        }

        /// <summary>
        /// Gets the selected language, defaults to English
        /// </summary>
        /// <returns>The language</returns>
        public static Language GetSelectedLanguage()
        {
            var selectedLanguage = Settings.Default.selectedLanguage;
            var languageCode = string.IsNullOrEmpty(selectedLanguage) ? "en-US" : selectedLanguage;
            return AvailableLanguages.First(l => l.Code == languageCode);
        }

        /// <summary>
        /// Checks for updates against GitHub
        /// </summary>
        internal async void CheckForUpdates()
        {
            await AutoUpdater.CheckForVersionUpdate();
            if (AutoUpdater.UpdateAvailable)
            {
                var notes = string.Empty;
                foreach (var release in AutoUpdater.Releases)
                {
                    notes += $"## {release.Version} - {release.Title} \r\n\r\n=== \r\n> ";
                    notes += release.Notes.Replace("- ","* ").Replace("\r\n", "\r\n> ");
                    notes += "\r\n=== \r\n";
                }
                var updateView = new Update(notes);
                updateView.Show();
            }
            else
            {
                GeneralHelper.WriteToConsole(Properties.Resources.NoUpdatesFound);
            }
        }

        /// <summary>
        /// Simple language model
        /// </summary>
        public class Language
        {
            /// <summary>
            /// The language model constructor
            /// </summary>
            /// <param name="name">The name of the language</param>
            /// <param name="code">The I18N code</param>
            /// <param name="locaPath">The location of the translation file</param>
            public Language(string name, string code, string locaPath)
            {
                Name = name;
                Code = code;
                LocaPath = locaPath;
            }

            public string Name { get; set; }
            public string Code { get; set; }
            public string LocaPath { get; set; }
        }

        #endregion
    }
}
