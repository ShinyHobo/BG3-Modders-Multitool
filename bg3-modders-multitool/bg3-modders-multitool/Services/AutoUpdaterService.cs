namespace bg3_modders_multitool.Services
{
    using Alphaleonis.Win32.Filesystem;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Diagnostics;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;

    /// <summary>
    /// Periodically checks GitHub for a new release of the application
    /// If found, prompts the user if they would like to upgrade
    /// If yes, downloads and replaces the currently installed version, then re-opens it
    /// If no, timer is shut off until the next time the application is started
    /// </summary>
    internal class AutoUpdaterService
    {
        public AutoUpdaterService() {
            PollGithub();
        }

        public AutoResetEvent AutoResetEvent { get; set; }
        public Timer Timer { get; set; }
        public HttpClient HttpClient { get; set; }
        private readonly string _repoUrl = "https://api.github.com/repositories/305852141/releases";

        /// <summary>
        /// Creates an HttpClient and timer to periodically check for new versions
        /// </summary>
        private void PollGithub()
        {
            HttpClient = new HttpClient();
            HttpClient.BaseAddress = new Uri(_repoUrl);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("BG3-Modders-Multitool");//Set the User Agent

            AutoResetEvent = new AutoResetEvent(true);
            Timer = new Timer(CheckForVersionUpdate, AutoResetEvent, 0, ((int)TimeSpan.FromMinutes(30).TotalMilliseconds));
        }

        /// <summary>
        /// Queries the GitHub release API endpoint for new versions
        /// Downloads a new version if one is found
        /// </summary>
        /// <param name="state">The state</param>
        private async void CheckForVersionUpdate(object state = null)
        {
            var releaseHistory = await HttpClient.GetAsync(_repoUrl);
            if (releaseHistory.StatusCode == HttpStatusCode.OK)
            {
                var currentVersion = GeneralHelper.GetAppVersion();
                string response = await releaseHistory.Content.ReadAsStringAsync();
                var releases = JsonConvert.DeserializeObject(response) as Newtonsoft.Json.Linq.JArray;
                if(releases != null)
                {
                    var newestRelease = releases.First();
                    if (newestRelease != null)
                    {
                        var matchedVersion = releases.FirstOrDefault(r => r["tag_name"].ToString().Remove(0, 1) == currentVersion);
                        var versionsBehind = releases.IndexOf(matchedVersion);
                        if (versionsBehind == -1 || versionsBehind > 0)
                        {
                            // release available
                            var newestTag = newestRelease["tag_name"].ToString().Remove(0, 1); // remove v
                            var assets = newestRelease["assets"];
                            if(assets != null)
                            {
                                DownloadNewVersion(assets);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Downloads and unzips the update into a temp directory
        /// </summary>
        /// <param name="assets">The asset list pulled from the release query</param>
        private void DownloadNewVersion(JToken assets)
        {
            var exeName = "bg3-modders-multitool";
            var asset = assets.FirstOrDefault(a => a["name"].ToString() == $"{exeName}.zip");
            if (asset != null)
            {
                var downloadUrl = asset["browser_download_url"].ToString();
                var tempZip = $"{DragAndDropHelper.TempFolder}\\update.zip";
                var updateDirectory = $"{DragAndDropHelper.TempFolder}\\Update";
                Directory.CreateDirectory(updateDirectory); // create directory if none exists
                File.Delete(tempZip); // clean temp zip if it exists
                File.Delete(Path.Combine(updateDirectory, exeName + ".exe")); // clean temp exe if it exists
                using (var client = new WebClient())
                {
                    client.DownloadFile(downloadUrl, tempZip);
                    using (ZipArchive archive = ZipFile.OpenRead(tempZip))
                    {
                        // only grabbing the main exe for now
                        foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains(exeName)))
                        {
                            entry.ExtractToFile(Path.Combine(updateDirectory, entry.FullName));
                        }
                    }
                    if(File.Exists(Path.Combine(updateDirectory, $"{exeName}.exe")))
                    {
                       ReplaceApplicationWithNewVersion();
                    } 
                    else
                    {
                        // TODO - failed to extract file
                    }
                }
            }
        }

        /// <summary>
        /// Closes the current application
        /// Starts a new background process
        /// Replaces the application with the new one
        /// Starts the application
        /// </summary>
        private void ReplaceApplicationWithNewVersion()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                System.Windows.Application.Current.Shutdown();
                var exeName = "bg3-modders-multitool.exe";
                var updatePath = $"{DragAndDropHelper.TempFolder}\\Update";
                var updateExe = Path.Combine(updatePath, exeName);
                var currentExe = Path.Combine(Directory.GetCurrentDirectory(), exeName);
                var process = new Process();
                process.StartInfo = new ProcessStartInfo("cmd.exe", $"/c replace {updateExe} {Directory.GetCurrentDirectory()} & rmdir {updatePath} /s /q & del {DragAndDropHelper.TempFolder}\\update.zip & start \"\" \"{currentExe}\"");
                process.Start();
            });
        }
    }
}
