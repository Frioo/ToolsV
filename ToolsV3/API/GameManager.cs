using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ToolsV3.API;

namespace ToolsV3
{
    public class GameManager
    {
        private static string GTA_REGISTRY_PATH = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V";
        private static string STEAM_REGISTRY_PATH_x86 = @"SOFTWARE\Valve\Steam";
        private static string STEAM_REGISTRY_PATH_x64 = @"SOFTWARE\WOW6432Node\Valve\Steam";

        public string InstallFolder { get; }
        public string ModStorageFolder { get; }
        public string PatchVersion { get; }
        public string Language { get; }
        public string CommandlinePath { get; }
        public bool IsSteam { get; }
        public bool IsModded { get; }
        public List<GameProperty> GameProperties { get; }

        public GameManager()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            Utils.Log("Initializing GTA manager...");

            string registryInstallPath = String.Empty;
            try
            {
                registryInstallPath = Registry.GetValue(GTA_REGISTRY_PATH, "InstallFolder", String.Empty).ToString();
            }
            catch(Exception ex)
            {
                MainWindow.ShowInitErrorAndExit();
            }
            if (registryInstallPath == String.Empty || registryInstallPath == null)
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
            this.Language = GTAFileVersionInfo.Language;

            if (File.Exists(this.InstallFolder + @"\commandline.txt"))
            {
                this.CommandlinePath = this.InstallFolder + @"\commandline.txt";
            }
            else
            {
                File.Create(this.InstallFolder + @"\commandline.txt");
                this.CommandlinePath = this.CommandlinePath + @"\commandline.txt";
                Utils.Log("Commandline.txt doesn't exitst, creating...");
            }

            if (!Directory.Exists(this.InstallFolder + Utils.MOD_STORAGE_FOLDER_ENDPOINT))
            {
                Utils.Log("ToolsV mods folder doesn't exist, creating...");
                Directory.CreateDirectory(this.InstallFolder + Utils.MOD_STORAGE_FOLDER_ENDPOINT);
            }
            this.ModStorageFolder = this.InstallFolder + Utils.MOD_STORAGE_FOLDER_ENDPOINT;

            this.IsModded = Directory.Exists(this.InstallFolder) && Directory.Exists(this.InstallFolder + @"\mods") ? Directory.GetFiles(this.InstallFolder + @"\mods").Length != 0 : false;
            if (!IsModded && GetMods(false).Count != 0 || GetModdedRPFs().Count != 0)
            {
                this.IsModded = true;
            }
            this.GameProperties = GetGameProperties();

            s.Stop();
            Utils.Log("Patch version: " + PatchVersion);
            Utils.Log("Language: " + Language);
            Utils.Log("Enabled mods: " + GetMods(false).Count);
            Utils.Log("Total mods: " + GetMods(true).Count);
            Utils.Log("RPF mods: " + GetModdedRPFs().Count);
            Utils.Log($"Game installation initialized in {Math.Round(s.Elapsed.TotalMilliseconds / 1000, 3)} seconds");
            s.Reset();
        }

        public void StartGame()
        {
            if (IsSteam)
            {
                Process.Start("steam://rungameid/271590");
            }
            else
            {
                Process.Start(InstallFolder + @"\PlayGTAV.exe");
            }
        }

        #region mod management
        public void EnableMods()
        {
            Utils.Log("Enabling all mods...");
            List<string> files = Directory.GetFiles(this.ModStorageFolder).ToList<string>();
            for (int i = 0; i < files.Count; i++)
            {
                File.Move(files[i], files[i].Replace(this.ModStorageFolder, this.InstallFolder));
            }
        }

        public void EnableMods(List<Mod> modsToEnable)
        {
            Utils.Log($"Enabling {modsToEnable.Count} mods...");
            for (int i = 0; i < modsToEnable.Count; i++)
            {
                Utils.Log($"moving {modsToEnable[i].Filename}");
                string src = ModStorageFolder + @"\" + modsToEnable[i].Filename;
                string dst = InstallFolder + @"\" + modsToEnable[i].Filename;
                Utils.Log($"source: {src}{Environment.NewLine}destination: {dst}");
                File.Move(src, dst);
            }
        }

        public void EnableMod(Mod mod)
        {
            Utils.Log($"Enabling {mod.Filename}...");
            string src = ModStorageFolder + @"\" + mod.Filename;
            string dst = InstallFolder + @"\" + mod.Filename;
            Utils.Log($"source: {src}{Environment.NewLine}destination: {dst}");
            File.Move(src, dst);
        }

        public void DisableMods()
        {
            Utils.Log("Disabling all mods...");
            List<Mod> mods = this.GetModFiles();
            for (int i = 0; i < mods.Count; i++)
            {
                Utils.Log($"moving {mods[i].Filename}");
                File.Move(InstallFolder + @"\" + mods[i].Filename, this.ModStorageFolder + @"\" + mods[i].Filename);
            }
        }

        public void DisableMods(List<Mod> modsToDisable)
        {
            Utils.Log($"Disabling {modsToDisable.Count} mods...");
            for (int i = 0; i < modsToDisable.Count; i++)
            {
                Utils.Log($"moving {modsToDisable[i].Filename}");
                string src = InstallFolder + @"\" + modsToDisable[i].Filename;
                string dst = ModStorageFolder + @"\" + modsToDisable[i].Filename;
                Utils.Log($"source: {src}{Environment.NewLine}destination: {dst}");
                File.Move(src, dst);
            }
        }

        public void DisableMod(Mod mod)
        {
            Utils.Log($"Disabling {mod.Filename}...");
            string src = InstallFolder + @"\" + mod.Filename;
            string dst = ModStorageFolder + @"\" + mod.Filename;
            Utils.Log($"source: {src}{Environment.NewLine}destination: {dst}");
            File.Move(src, dst);
        }

        public List<Mod> GetMods(bool includeDisabled)
        {
            List<Mod> mods = GetModFiles();
            if (includeDisabled)
            {
                string[] disabledModFiles = Directory.GetFiles(this.ModStorageFolder);
                for (int i = 0; i < disabledModFiles.Length; i++)
                {
                    string filename = disabledModFiles[i].Replace(this.ModStorageFolder, String.Empty);
                    mods.Add(new Mod(disabledModFiles[i], ModStorageFolder, false));
                }
            }

            return mods;
        }

        private List<Mod> GetModFiles()
        {
            string[] files = Directory.GetFiles(this.InstallFolder);
            List<Mod> result = new List<Mod>();
            for (int i = 0; i < files.Length; i++)
            {
                string filename = files[i].Replace(this.InstallFolder + @"\", String.Empty);
                if (filename.ToLower().Contains(@".asi") || filename.ToLower().Contains(@"scripthookv.dll") || filename.ToLower().Contains("dsound") || filename.ToLower().Contains(@"dinput8.dll"))
                {
                    Mod mod = new Mod(files[i], this.InstallFolder, true);
                    result.Add(mod);
                }
            }
            return result;
        }

        private List<Mod> GetModdedRPFs()
        {
            if (IsModded)
            {
                string[] moddedRPFPaths = Directory.GetFiles(this.InstallFolder + @"\mods");
                List<Mod> moddedRPFList = new List<Mod>();
                for (int i = 0; i < moddedRPFPaths.Length; i++)
                {
                    Mod moddedRPF = new Mod(moddedRPFPaths[i], this.InstallFolder, true);
                    moddedRPFList.Add(moddedRPF);
                }
                return moddedRPFList;
            }
            else
            {
                return new List<Mod>();
            }
        }
        #endregion

        public bool IsScriptHookCompatible()
        {
            return true;
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
