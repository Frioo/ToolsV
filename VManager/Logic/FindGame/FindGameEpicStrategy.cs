using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsV.Models;

namespace ToolsV.Logic.FindGame
{
    internal class FindGameEpicStrategy : IFindGameStrategy
    {
        private static string EPIC_REG_KEY = @"SOFTWARE\WOW6432Node\Epic Games\EpicGamesLauncher";

        public string? FindGameDirectory()
        {
            var epicGamesLauncherPath = GetEpicGamesClientDirectory();

            if (string.IsNullOrEmpty(epicGamesLauncherPath)) { return null; }

            var manifestsPath = Path.Combine(epicGamesLauncherPath, "Manifests");
            var manifests = Directory.GetFiles(manifestsPath, "*.item");

            foreach (var path in manifests)
            {
                var content = File.ReadAllText(path);
                var gamePath = JObject.Parse(content)["InstallLocation"]?.Value<string>();

                if (!string.IsNullOrEmpty(gamePath) && gamePath.Contains("Grand Theft Auto V"))
                {
                    return gamePath;
                }
            }

            return null;
        }

        private static string? GetEpicGamesClientDirectory()
        {
            var path = Registry.LocalMachine
                .OpenSubKey(EPIC_REG_KEY)?
                .GetValue("AppDataPath")?
                .ToString();

            return path;
        }
    }
}
