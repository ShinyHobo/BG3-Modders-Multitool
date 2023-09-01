namespace bg3_modders_multitool.Services
{
    using Newtonsoft.Json;
    using System;
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

        private void PollGithub()
        {
            HttpClient = new HttpClient();
            HttpClient.BaseAddress = new Uri(_repoUrl);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("BG3-Modders-Multitool");//Set the User Agent

            AutoResetEvent = new AutoResetEvent(true);
            Timer = new Timer(CheckForVersionUpdate, AutoResetEvent, 0, ((int)TimeSpan.FromSeconds(5).TotalMilliseconds));
        }

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
                            GeneralHelper.WriteToConsole($"{newestTag} is available");
                            var assets = matchedVersion["assets"];
                            if(assets != null)
                            {
                                var asset = assets.FirstOrDefault(a => a["name"].ToString() == "bg3-modders-multitool.zip");
                                if (asset != null)
                                {
                                    var downloadUrl = asset["browser_download_url"].ToString();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
