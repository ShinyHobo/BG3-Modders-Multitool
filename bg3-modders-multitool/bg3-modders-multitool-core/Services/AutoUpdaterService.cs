namespace bg3_modders_multitool.Services
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.ViewModels;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Periodically checks GitHub for a new release of the application
    /// If found, prompts the user if they would like to upgrade
    /// If yes, downloads and replaces the currently installed version, then re-opens it
    /// If no, timer is shut off until the next time the application is started
    /// </summary>
    public class AutoUpdaterService : BaseViewModel
    {
        public AutoUpdaterService(MainWindow mainWindow) {
            _mainWindow = mainWindow;
            PollGithub();
        }

        private MainWindow _mainWindow { get; set; }
        public AutoResetEvent AutoResetEvent { get; set; }
        public Timer Timer { get; set; }
        public HttpClient HttpClient { get; set; }
        private bool _updateAvailable;
        public bool UpdateAvailable { get { return _updateAvailable; } 
            set { _updateAvailable = value; _mainWindow.UpdateVisible = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden; } 
        }
        public List<Release> Releases { get; private set; }
        public bool UnknownVersion { get; private set; }

        private readonly string _repoUrl = "https://api.github.com/repositories/305852141/releases";
        private readonly string _exeName = "bg3-modders-multitool";

        /// <summary>
        /// Creates an HttpClient and timer to periodically check for new versions
        /// </summary>
        private void PollGithub()
        {
            Releases = new List<Release>();

            HttpClient = new HttpClient();
            HttpClient.BaseAddress = new Uri(_repoUrl);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("BG3-Modders-Multitool");//Set the User Agent

            AutoResetEvent = new AutoResetEvent(true);
            Timer = new Timer(CheckForVersionUpdate, AutoResetEvent, 0, ((int)TimeSpan.FromMinutes(15).TotalMilliseconds));
        }

        /// <summary>
        /// Queries the GitHub release API endpoint for new versions
        /// Downloads a new version if one is found
        /// </summary>
        /// <param name="state">The state</param>
        private async void CheckForVersionUpdate(object state)
        {
            await CheckForVersionUpdate();
        }

        /// <summary>
        /// Queries the GitHub release API endpoint for new versions
        /// Downloads a new version if one is found
        /// </summary>
        /// <returns>The release check task</returns>
        public Task CheckForVersionUpdate(bool changelog = false)
        {
            return Task.Run(async () => {
                try
                {
                    var releaseHistory = await HttpClient.GetAsync(_repoUrl);
                    if (releaseHistory.StatusCode == HttpStatusCode.OK)
                    {
                        var currentVersion = GeneralHelper.GetAppVersion();
                        string response = await releaseHistory.Content.ReadAsStringAsync();
                        var releases = JsonConvert.DeserializeObject(response) as Newtonsoft.Json.Linq.JArray;
                        if (releases != null)
                        {
                            var newestRelease = releases.FirstOrDefault();
                            if (newestRelease != null || changelog)
                            {
                                var matchedVersion = releases.FirstOrDefault(r => r["tag_name"].ToString().Remove(0, 1) == currentVersion);
                                UnknownVersion = matchedVersion == null;
                                var versionsBehind = releases.IndexOf(matchedVersion);
                                versionsBehind = versionsBehind == -1 ? releases.Count : versionsBehind;
                                if (versionsBehind > 0 || changelog)
                                {
                                    if(!changelog)
                                        UpdateAvailable = true;
                                    Releases.Clear();

                                    versionsBehind = changelog ? releases.Count : versionsBehind;

                                    for (int i = 0; i < versionsBehind; i++)
                                    {
                                        var releaseAsset = releases[i];
                                        var version = releaseAsset["tag_name"].ToString().Remove(0, 1);
                                        var releaseNotes = releaseAsset["body"].ToString();
                                        var exeAsset = releaseAsset["assets"].FirstOrDefault(a => a["name"].ToString() == $"{_exeName}.zip");
                                        var title = releaseAsset["name"].ToString();
                                        var downloadUrl = exeAsset?["browser_download_url"].ToString();
                                        var release = new Release(version, title, releaseNotes, downloadUrl);
                                        Releases.Add(release);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        GeneralHelper.WriteToConsole(Properties.Resources.FailedToFetchUpdates, (int)releaseHistory.StatusCode);
                    }
                    return;
                }
                catch (Exception e)
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.FailedToFetchUpdates, e.Message);
                }
            });
        }

        /// <summary>
        /// Downloads and unzips the update into a temp directory
        /// </summary>
        public void Update()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Timer.Dispose();
                AutoResetEvent.Dispose();
                System.Windows.Application.Current.MainWindow.IsEnabled = false;
                System.Windows.Application.Current.Shutdown();

                var newestRelease = Releases.First();

                var tempZip = $"{DragAndDropHelper.TempFolder}\\update.zip";
                var updateDirectory = $"{DragAndDropHelper.TempFolder}\\Update";

                // Create and/or clean temp directory
                Directory.CreateDirectory(updateDirectory);
                File.Delete(tempZip);
                File.Delete(Path.Combine(updateDirectory, _exeName + ".exe"));

                using (var client = new WebClient())
                {
                    client.DownloadFile(newestRelease.DownloadUrl, tempZip);
                    using (ZipArchive archive = ZipFile.OpenRead(tempZip))
                    {
                        // only grabbing the main exe for now
                        foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains(_exeName)))
                        {
                            entry.ExtractToFile(Path.Combine(updateDirectory, entry.FullName));
                        }
                    }
                    if (File.Exists(Path.Combine(updateDirectory, $"{_exeName}.exe")))
                    {
                        ReplaceApplicationWithNewVersion();
                    }
                    else
                    {
                        // TODO - failed to extract file
                    }
                }
            });
            
        }

        /// <summary>
        /// Closes the current application
        /// Starts a new background process
        /// Replaces the application with the new one
        /// Starts the application
        /// </summary>
        private void ReplaceApplicationWithNewVersion()
        {
            var exeName = "bg3-modders-multitool.exe";
            var updatePath = $"{DragAndDropHelper.TempFolder}\\Update";
            var updateExe = Path.Combine(updatePath, exeName);
            var currentExe = Path.Combine(Directory.GetCurrentDirectory(), exeName);
            var process = new Process();
            process.StartInfo = new ProcessStartInfo("cmd.exe",
                $"/c echo {Properties.Resources.ClosingAppForUpdate} " +
                $"& C:\\Windows\\System32\\ping.exe -4 -n 5 \"\">nul " +
                $"& copy /b/v/y \"{updateExe}\" \"{Directory.GetCurrentDirectory()}\" " +
                $"& rmdir \"{updatePath}\" /s /q " +
                $"& del \"{DragAndDropHelper.TempFolder}\\update.zip\" " +
                $"& start \"\" \"{currentExe}\"");
            process.Start();
        }
    }

    /// <summary>
    /// GitHub release information
    /// </summary>
    public class Release
    {
        public Release(string version, string title, string notes, string downloadUrl) { 
            Version = version;
            Title = title;
            Notes = notes;
            DownloadUrl = downloadUrl;
        }

        public string Version { get; private set; }
        public string Title { get; private set; }
        public string Notes { get; private set;}
        public string DownloadUrl { get; private set;}
    }
}
