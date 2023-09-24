/// <summary>
/// Service helper for get the paths for the mods and player profiles folders.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using bg3_modders_multitool.Properties;
    using System;
    using System.IO;

    public class PathHelper
    {
        private static readonly PathHelper _instance = new PathHelper();
        public static PathHelper Instance => _instance;

        public static string ModsFolderPath { get; private set; }
        public static string PlayerProfilesFolderPath { get; private set; }

        private readonly string _companyName = "Larian Studios";
        private readonly string _gameName = "Baldur's Gate 3";
        private readonly string _modsFolderName = "Mods";
        private readonly string _playerProfilesFolderName = "PlayerProfiles";

        private bool _ignoreSettingsPropertyChangedEvent = false;

        private PathHelper()
        {
            // We subscribe to the PropertyChanged event of the Settings.
            // When gameDocumentsPath changes, we reinitialize the paths.
            Settings.Default.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName.Equals(nameof(Settings.gameDocumentsPath)))
                {
                    if(!_ignoreSettingsPropertyChangedEvent)
                        InitializePaths();

                    // If flag is on, reset the flag so we can process the event next time.
                    _ignoreSettingsPropertyChangedEvent = false;
                }
            };

            InitializePaths();
        }

        /// <summary>
        /// Initializes the paths for the mods and player profiles folders based on game documents path.
        /// If not defined, it will try to find the game documents path automatically using the default installation location.
        /// </summary>
        public void InitializePaths()
        {
            // Clear any value that might have been set before...
            ModsFolderPath = string.Empty;
            PlayerProfilesFolderPath = string.Empty;

            // Gets the current config value for the game path..
            var gameDocumentsPath = Settings.Default.gameDocumentsPath;
            
            if (string.IsNullOrEmpty(gameDocumentsPath))
            {
                // As the config value is empty, we'll try to find the game path automatically by
                // looking for the default location..
                gameDocumentsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify),
                    _companyName,
                    _gameName
                );
            }

            // In case the game path is still empty we exit.
            if (!Directory.Exists(gameDocumentsPath))
                return;
            
            // Set the paths for the mods and player profiles folders..
            ModsFolderPath = Path.Combine(gameDocumentsPath, _modsFolderName);
            PlayerProfilesFolderPath = Path.Combine(gameDocumentsPath, _playerProfilesFolderName);

            // Most of times gameDocumentsPath will be the same as the config value,
            // however, there is a special case on first run, where the config does not
            // have a value, and the default installation path is used instead. When
            // this happens, we need to update the config value with the new path.
            if (!gameDocumentsPath.Equals(Settings.Default.gameDocumentsPath))
            {
                // Flags the event handler so we skip the event processing in subscription.
                _ignoreSettingsPropertyChangedEvent = true; 

                Settings.Default.gameDocumentsPath = gameDocumentsPath;
                Settings.Default.Save();
            }
        }
    }
}