using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;

namespace ToolsV3.API
{
    public static class Updater
    {
        public static readonly string VERSION_TAG = AssemblyName.GetAssemblyName(Utils.ExecutableFilePath).Version.ToString();
        private static readonly string CHANGELOG_URL = @"https://pastebin.com/raw/RvxX60E4";
        private static readonly string GITHUB_API_URL = @"https://api.github.com/";
        private static readonly string GITHUB_API_RATE_LIMIT_URL = GITHUB_API_URL + @"rate_limit";
        private static readonly string GITHUB_REPO_URL = @"repos/Frioo/ToolsV/";
        private static readonly string GITHUB_TAGS_URL = GITHUB_API_URL + GITHUB_REPO_URL + @"tags";
        private static readonly string GITHUB_LATEST_RELEASE_URL = GITHUB_API_URL + GITHUB_REPO_URL + @"releases/latest";
        private static string GITHUB_RELEASE_BY_TAG(string tag)
        {
            return UrlWithQuery(GITHUB_API_URL + GITHUB_REPO_URL + @"releases/tags/" + tag);
        }
        private static readonly string UPDATE_FILE_ENDPOINT = @"\ToolsV.exe";
        private static MetroWindow view { get; set; }

        private static HttpClient client = new HttpClient();


        public static void Initialize()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "ToolsV");
            Utils.Log($"ToolsV version: {VERSION_TAG}");
        }

        public static void OpenDownloadSites()
        {
            Process.Start(@"https://github.com/Frioo/ToolsV/releases");
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
            await CheckApiRate();
            string latestTag = "";
            try
            {
                Utils.Log("Updater: get latest tag");
                JArray tags = JArray.Parse(await client.GetStringAsync(UrlWithQuery(GITHUB_TAGS_URL)));
                latestTag = tags[0].ToObject<JObject>().GetValue("name").ToString();
                Utils.Log($"latest tag: {latestTag}");
            }
            catch(Exception e)
            {
                Utils.Log(e.StackTrace);
            }

            return latestTag;
        }

        public static async Task<string> CheckApiRate()
        {
            Utils.Log("Updater: check API rate");
            JObject rate = JObject.Parse(await client.GetStringAsync(UrlWithQuery(GITHUB_API_RATE_LIMIT_URL)));
            string limit = rate["resources"]["core"]["limit"].ToString();
            string remaining = rate["resources"]["core"]["remaining"].ToString();
            Utils.Log($"API rate: {remaining}/{limit}");
            return $"{remaining}/{limit}";
        }

        private static int ExtractVersion(string tag)
        {
            return Int32.Parse(string.Join(string.Empty, Regex.Matches(tag, @"\d+").OfType<Match>().Select(m => m.Value)));
        }

        private static string UrlWithQuery(string url)
        {
            var builder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["client_id"] = Authorization.Client.Id;
            query["client_secret"] = Authorization.Client.Secret;
            builder.Query = query.ToString();
            return builder.ToString();
        }

        #region dialogs
        public static async Task ShowUpdateAvailableDialog(MetroWindow window, string latestVersion)
        {
            var res = await window.ShowMessageAsync("Update available", "A newer version of ToolsV is available, would you like to download it?\n" +
                "Current version: " + Updater.VERSION_TAG +
                "\nLatest version: " + latestVersion, MessageDialogStyle.AffirmativeAndNegative);
            if (res == MessageDialogResult.Affirmative)
            {
                view = window;
                Utils.Log("Update dialog: opening download pages...");
                OpenDownloadSites();
            }
            else
            {
                Utils.Log("Update dialog: update postponed");
            }
        }

        public static async Task ShowNoUpdateAvailableDialog(MetroWindow window)
        {
            var res = await window.ShowMessageAsync("No updates found", "You have the latest version of ToolsV.\n" +
                "Current version: " + Updater.VERSION_TAG, MessageDialogStyle.Affirmative);
        }
        #endregion
    }
}
