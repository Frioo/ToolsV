﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ToolsV3
{
    public static class Utils
    {
        private static string TAG = @"ToolsV:Log -> ";
        public static string MOD_FOLDER_ENDPOINT = @"\mods";
        public static string SCRIPT_FOLDER_ENDPOINT = @"\scripts";
        public static string MOD_STORAGE_FOLDER_ENDPOINT = @"\ToolsV\Mods";
        public static string ExecutableDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }
        public static string ExecutableFilePath
        {
            get
            {
                return Assembly.GetEntryAssembly().Location;
            }
        }
        public static string UpdaterFilePath
        {
            get
            {
                return ExecutableDirectory + @"\Updater.exe";
            }
        }

        public static string GITHUB_REPO_PAGE_URL = @"https://github.com/Frioo/ToolsV";
        public static string GTA5MODS_PAGE_URL =
            @"https://www.gta5-mods.com/tools/toolsv-launcher-mod-manager-toolpack";

        public enum LaunchMode
        {
            NORMAL = 1,
            SINGLEPLAYER_WITH_MODS = 2,
            SINGLEPLAYER_WITHOUT_MODS = 3,
            ONLINE = 4
        }

        public enum ModType
        {
            NATIVE = 1,
            SCRIPT = 2,
            RPF = 3
        }

        public static void Log(string text)
        {
            Debug.WriteLine(TAG + text);
        }

        public static string GetExecutableDirectory()
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Log($"Utils: detected executable directory: {path}");
            return path;
        }

        public static string GetExecutablePath()
        {
            string path = Assembly.GetEntryAssembly().Location;
            Log($"Utils: detected executable path: {path}");
            return path;
        }

        public static string GetProgramVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static int ExtractVersion(string tag)
        {
            return int.Parse(string.Join(string.Empty, Regex.Matches(tag, @"\d+").OfType<Match>().Select(m => m.Value)));
        }
    }
}
