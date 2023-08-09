using bg3_modders_multitool.Properties;
using System;
using System.IO;

namespace bg3_modders_multitool.Services
{
    public class PathHelper
    {
        private static readonly PathHelper _instance = new PathHelper();
        public static PathHelper Instance => _instance;

        public static string ModsFolderPath { get; private set; }
        public static string PlayerProfilesFolderPath { get; private set; }

        private PathHelper()
        {
            InitializePaths();
        }

        private void InitializePaths()
        {
            try
            {
                // Gets the current config value for the game path..
                var gameDocumentsPath = Settings.Default.gameDocumentsPath;

                if (string.IsNullOrEmpty(gameDocumentsPath))
                {
                    // As the config value is empty, we'll try to find the game path automatically by
                    // looking for the default location..
                    gameDocumentsPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify),
                        "Larian Studios",
                        "Baldur's Gate 3"
                    );
                }

                // In case the game path is still empty, we'll throw an exception...
                if (!Directory.Exists(gameDocumentsPath))
                    throw new Exception($"Game Documents path not found at '{gameDocumentsPath}'.");

                // Set the paths for the mods and player profiles folders
                // and create them if they don't exist..
                ModsFolderPath = Path.Combine(gameDocumentsPath, "Mods");
                if (!Directory.Exists(ModsFolderPath))
                    Directory.CreateDirectory(ModsFolderPath);

                PlayerProfilesFolderPath = Path.Combine(gameDocumentsPath, "PlayerProfiles");
                if (!Directory.Exists(PlayerProfilesFolderPath))
                    Directory.CreateDirectory(PlayerProfilesFolderPath);

                // If the game path has changed, we update the config value...
                if (!gameDocumentsPath.Equals(Settings.Default.gameDocumentsPath))
                {
                    Settings.Default.gameDocumentsPath = gameDocumentsPath;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                GeneralHelper.WriteToConsole(ex.Message);
            }
        }
    }
}