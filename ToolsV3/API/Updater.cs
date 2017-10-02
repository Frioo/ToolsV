using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToolsV3.API
{
    public static class Updater
    {
        public static readonly string VERSION_TAG = @"v3.0.0-ALPHA";
        private static readonly string CHANGELOG_URL = @"https://pastebin.com/raw/RvxX60E4";
        private static readonly string GITHUB_API_URL = @"https://api.github.com/";
        private static readonly string GITHUB_REPO_URL = @"repos/Frioo/ToolsV";
        private static readonly string GITHUB_TAGS_URL = GITHUB_API_URL + GITHUB_REPO_URL + @"tags";
        private static readonly string GITHUB_LATEST_RELEASE_URL = GITHUB_API_URL + GITHUB_REPO_URL + @"releases/latest";
        private static string GITHUB_RELEASE_BY_TAG(string tag)
        {
            return GITHUB_API_URL + GITHUB_REPO_URL + @"releases/tags/" + tag;
        }
        private static readonly string UPDATE_FILE_ENDPOINT = @"\ToolsV.exe";
        private static MetroWindow view { get; set; }

        private static HttpClient client = new HttpClient();


        public static void Initialize()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "ToolsV");
        }

        public static async Task Update()
        {
            Utils.Log("Updater: update");
            string newFilePath = await DownloadLatestExecutable();
            // Extract Updater from resources
            File.WriteAllBytes(Utils.UpdaterFilePath, Properties.Resources.Updater);
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Utils.UpdaterFilePath;
            info.Arguments = $"-o {Utils.ExecutableFilePath} -n {newFilePath}";
            info.Verb = "runas";
            Process.Start(info);
        }

        public static async Task<string> DownloadLatestExecutable()
        {
            JObject root = JObject.Parse(await client.GetStringAsync(GITHUB_RELEASE_BY_TAG(await GetLatestTag())));
            string fileUrl = (string)root["assets"][0]["browser_download_url"];
            string currentDir = Utils.GetExecutableDirectory();
            Utils.Log($"downloading latest executable...{Environment.NewLine}url: {fileUrl}");
            try
            {
                using (var downloader = new WebClient())
                {
                    var controller = await view.ShowProgressAsync("Downloading latest release", $"This shouldn't take too long.{Environment.NewLine}");
                    controller.Minimum = 0;
                    controller.Maximum = 100;
                    controller.SetCancelable(false);
                    downloader.DownloadFileCompleted += (sender, e) => Downloader_DownloadFileCompleted(sender, e, controller);
                    downloader.DownloadProgressChanged += (sender, e) => Downloader_DownloadProgressChanged(sender, e, controller);
                    await downloader.DownloadFileTaskAsync(new Uri(fileUrl), currentDir + UPDATE_FILE_ENDPOINT);
                }
            }
            catch (Exception ex)
            {
                Utils.Log("download failed");
                Utils.Log(ex.Message);
                await ShowUpdateFailedDialog(view);
            }
            return currentDir + UPDATE_FILE_ENDPOINT;
        }

        private static void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, ProgressDialogController controller)
        {
            controller.SetMessage($"This shouldn't take too long.{Environment.NewLine}File size: {e.TotalBytesToReceive / 1024 / 1024}MB");
            controller.SetProgress(e.ProgressPercentage);
        }

        private static async void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e, ProgressDialogController controller)
        {
            await controller.CloseAsync();
            if (e.Cancelled)
            {
                Utils.Log("download canceled");
            }
            else
            {
                Utils.Log("download completed!");
            }
        }

        public static async Task<string> GetChangelog()
        {
            string changelog = "";

            try
            {
                Utils.Log("Core: download changelog");
                Utils.Log("downloading changelog...");
                changelog = await client.GetStringAsync(CHANGELOG_URL);
                Utils.Log("changelog downloaded successfully");
            }
            catch (Exception e)
            {
                Utils.Log("downloading changelog failed");
                Utils.Log(e.Message);
                changelog = "Unable to download changelog";
            }

            return changelog;
        }

        public static async Task<bool> GetIsLatest()
        {
            bool isLatest = true;
            try
            {
                Utils.Log("Updater: check for update");
                string latest = await GetLatestTag();
                isLatest = ExtractVersion(latest) <= ExtractVersion(VERSION_TAG);
                Utils.Log(ExtractVersion(latest).ToString());
                if (isLatest)
                {
                    Utils.Log("no updates found");
                }
                else
                {
                    Utils.Log("newer version is available");
                }
            }
            catch (Exception ex)
            {
                Utils.Log("update check failed");
                Utils.Log(ex.Message);
            }

            return isLatest;
        }

        public static async Task<string> GetLatestTag()
        {
            string latestTag = "";
            try
            {
                Utils.Log("Updater: get latest tag");
                string json = await client.GetStringAsync(GITHUB_TAGS_URL);
                JArray tags = JArray.Parse(await client.GetStringAsync(GITHUB_TAGS_URL));
                latestTag = tags[0].ToObject<JObject>().GetValue("name").ToString();
                Utils.Log($"latest tag: {latestTag}");
            }
            catch(Exception e)
            {
                Utils.Log(e.StackTrace);
            }

            return latestTag;
        }

        private static int ExtractVersion(string tag)
        {
            return Int32.Parse(string.Join(string.Empty, Regex.Matches(tag, @"\d+").OfType<Match>().Select(m => m.Value)));
        }

        #region dialogs
        public static async Task ShowUpdateAvailableDialog(MetroWindow window, string latestVersion)
        {
            MessageDialogResult res = await window.ShowMessageAsync("Update available", "A newer version of ToolsV is available, would you like to download it?\n" +
                "Current version: " + Updater.VERSION_TAG +
                "\nLatest version: " + latestVersion, MessageDialogStyle.AffirmativeAndNegative);
            if (res == MessageDialogResult.Affirmative)
            {
                view = window;
                Utils.Log("Update dialog: engaging automated self-update");
                await Update();
            }
            else
            {
                Utils.Log("Update dialog: update postponed");
            }
        }

        public static async Task ShowNoUpdateAvailableDialog(MetroWindow window)
        {
            MessageDialogResult res = await window.ShowMessageAsync("No updates found", "You have the latest version of ToolsV.\n" +
                "Current version: " + Updater.VERSION_TAG, MessageDialogStyle.Affirmative);
        }

        public static async Task ShowUpdateFailedDialog(MetroWindow window)
        {
            MessageDialogResult res = await window.ShowMessageAsync("Update failed :(", "Something went wrong, you can try downloading the latest version manually.",
                MessageDialogStyle.Affirmative);
        }
        #endregion
    }
}
