using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToolsV3.API;

namespace ToolsV3
{
    public class GameManager
    {
        private static string GTA_REGISTRY_PATH = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V";
        private static string STEAM_REGISTRY_PATH_x86 = @"SOFTWARE\Valve\Steam";
        private static string STEAM_REGISTRY_PATH_x64 = @"SOFTWARE\WOW6432Node\Valve\Steam";

        public string InstallFolder { get; }
        public string PatchVersion { get; }
        public string Language { get; }
        public bool IsSteam { get; }
        public bool IsModded { get; }
        public List<Mod> Mods { get; }
        public List<Mod> ModdedRPFs { get; }
        public List<GameProperty> GameProperties { get; }

        public GameManager()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            Utils.Log("Initializing GTA manager...");
            string registryInstallPath = Registry.GetValue(GTA_REGISTRY_PATH, "InstallFolder", String.Empty).ToString();
            if (registryInstallPath == String.Empty)
            {
                this.IsSteam = true;
                this.InstallFolder = GetSteamGTAInstallationFolder();
                Utils.Log("Game found! Edition: Steam");
                Utils.Log("Installation folder: " + InstallFolder);
            }
            else
            {
                this.IsSteam = false;
                this.InstallFolder = registryInstallPath;
                Utils.Log("Game found! Edition: Rockstar Warehouse");
                Utils.Log("Installation folder: " + InstallFolder);
            }

            if (string.IsNullOrEmpty(this.InstallFolder))
            {
                return;
            }
            FileVersionInfo GTAFileVersionInfo = FileVersionInfo.GetVersionInfo(registryInstallPath + @"\GTA5.exe");
            this.PatchVersion = GTAFileVersionInfo.ProductVersion;
            Utils.Log("Patch version: " + PatchVersion);
            this.Language = GTAFileVersionInfo.Language;
            Utils.Log("Language: " + Language);
            this.IsModded = Directory.Exists(this.InstallFolder) && Directory.Exists(this.InstallFolder + @"\mods") ? Directory.GetFiles(this.InstallFolder + @"\mods").Length != 0 : false;
            this.Mods = GetMods();
            Utils.Log("Mods found: " + Mods.Count);
            this.ModdedRPFs = GetModdedRPFs();
            Utils.Log("RPF mods: " + ModdedRPFs.Count);
            if (!IsModded && this.Mods.Count != 0 || this.ModdedRPFs.Count != 0)
            {
                this.IsModded = true;
            }
            this.GameProperties = GetGameProperties();
            s.Stop();
            Utils.Log("Game installation initialized in " + Math.Round(s.Elapsed.TotalMilliseconds / 1000, 3) + " seconds");
            s.Reset();
        }

        private List<GameProperty> GetGameProperties()
        {
            List<GameProperty> properties = new List<GameProperty>();
            properties.Add(new GameProperty("Install folder", this.InstallFolder));
            properties.Add(new GameProperty("Patch version", this.PatchVersion));
            properties.Add(new GameProperty("Language", Language));
            properties.Add(new GameProperty("Edition", IsSteam ? "Steam" : "Rockstar Warehouse"));
            properties.Add(new GameProperty("Vanilla", IsModded ? "No" : "Yes"));
            return properties;
        }

        private List<Mod> GetMods()
        {
            string[] gtaFiles = Directory.GetFiles(this.InstallFolder);
            List<Mod> mods = new List<Mod>();
            for (int i = 0; i < gtaFiles.Length; i++)
            {
                string filename = gtaFiles[i].Replace(this.InstallFolder, String.Empty);
                if (filename.ToLower().Contains(@".asi") || filename.ToLower().Equals(@"scripthookv.dll") || filename.ToLower().Equals(@"dinput8.dll"))
                {
                    Mod mod = new Mod(gtaFiles[i], this.InstallFolder);
                    mods.Add(mod);
                }
            }
            return mods;
        }

        private List<Mod> GetModdedRPFs()
        {
            if (IsModded)
            {
                string[] moddedRPFPaths = Directory.GetFiles(this.InstallFolder + @"\mods");
                List<Mod> moddedRPFList = new List<Mod>();
                for (int i = 0; i < moddedRPFPaths.Length; i++)
                {
                    Mod moddedRPF = new Mod(moddedRPFPaths[i], this.InstallFolder);
                    moddedRPFList.Add(moddedRPF);
                }
                return moddedRPFList;
            }
            else
            {
                return new List<Mod>();
            }
        }

        private string GetSteamGTAInstallationFolder()
        {
            string steamPath = GetSteamInstallationFolder();
            if (Directory.Exists(steamPath + @"\steamapps\common"))
            {
                Directory.CreateDirectory(steamPath + @"\steamapps\common");
            }
            var gameFolders = GetSteamGameFolders().Select(x => x + @"\steamapps\common");
            foreach (var gameFolder in gameFolders)
            {
                try
                {
                    var matches = Directory.GetDirectories(gameFolder, "Grand Theft Auto V");
                    if (matches.Length >= 1)
                    {
                        return matches[0];
                    }
                }
                catch (DirectoryNotFoundException ex)
                {
                    Utils.Log(ex.Message);
                }
            }
            return String.Empty;
        }

        private string GetSteamInstallationFolder()
        {
            RegistryKey steamKey = Registry.LocalMachine.OpenSubKey(STEAM_REGISTRY_PATH_x86) ?? Registry.LocalMachine.OpenSubKey(STEAM_REGISTRY_PATH_x64);
            return steamKey.GetValue("InstallPath").ToString();
        }

        private List<string> GetSteamGameFolders()
        {
            List<string> folders = new List<string>();

            string steamFolder = GetSteamInstallationFolder();
            folders.Add(steamFolder);

            string configFile = steamFolder + @"\config\config.vdf";

            Regex regex = new Regex("BaseInstallFolder[^\"]*\"\\s*\"([^\"]*)\"");
            using (StreamReader reader = new StreamReader(configFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        folders.Add(Regex.Unescape(match.Groups[1].Value));
                    }
                }
            }
            return folders;
        }
    }
}
