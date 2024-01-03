using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using ToolsV.Models;

namespace ToolsV.Logic.FindGame
{
    public class FindGameSteamStrategy : IFindGameStrategy
    {
        private static string STEAM_REGISTRY_PATH_x86 = @"Software\Valve\Steam";
        private static string STEAM_REGISTRY_PATH_x64 = @"SOFTWARE\WOW6432Node\Valve\Steam";

        public FindGameSteamStrategy() { }

        public string FindGameDirectory()
        {
            return GetSteamGameInstallationFolder();
        }

        private static string GetSteamGameInstallationFolder()
        {
            var matches = new List<string>();
            var gameFolders = GetSteamGameFolders();

            foreach (var gameFolder in gameFolders)
            {
                try
                {
                    matches.AddRange(Directory.GetDirectories(gameFolder, "Grand Theft Auto V"));
                }
                catch (Exception ex)
                {
                    // :(
                    //Utils.Log($"GameManager: GetSteamInstallationFolder -> {ex.Message}");
                    //MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            foreach (var match in matches)
            {
                if (File.Exists(match + @"\GTA5.exe"))
                {
                    return match;
                }
            }

            return string.Empty;
        }

        private static string? GetSteamClientInstallationFolder()
        {
            var path = GetSteam64Directory();

            if (!string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            return GetSteam32Directory();
        }

        private static string? GetSteam64Directory()
        {
            var registryKey = Registry.LocalMachine.OpenSubKey(STEAM_REGISTRY_PATH_x64);

            if (registryKey == null)
            {
                return null;
            }

            var path = registryKey.GetValue("InstallPath")?.ToString();

            return path;
        }

        private static string? GetSteam32Directory()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(STEAM_REGISTRY_PATH_x86);

            if (registryKey == null)
            {
                return null;
            }

            var path = registryKey.GetValue("SteamPath")?.ToString();

            return path;
        }

        private static List<string> GetSteamGameFolders()
        {
            var steamFolder = GetSteamClientInstallationFolder();
            var configFile = steamFolder + @"\config\config.vdf";
            var res = new List<string>();
            if (Directory.Exists(steamFolder + @"/steamapps/common"))
            {
                res.Add(steamFolder + @"/steamapps/common");
            }

            var regex = new Regex("BaseInstallFolder[^\"]*\"\\s*\"([^\"]*)\"");
            using (var reader = new StreamReader(configFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        res.Add(Regex.Unescape(match.Groups[1].Value) + @"/steamapps/common");
                    }
                }
            }
            return res;
        }
    }
}
