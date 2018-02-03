using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using ToolsV3.API;

namespace ToolsV3
{
    public class GameManager
    {
        private static string GTA_REGISTRY_PATH = @"SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V";
        private static string STEAM_REGISTRY_PATH_x86 = @"Software\Valve\Steam";
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
            var s = new Stopwatch();
            s.Start();
            Utils.Log("Initializing GTA manager...");

            var gamePath = GetSteamGameInstallationFolder();
            if (gamePath.Equals(string.Empty))
            {
                gamePath = GetRockstarGameInstallationFolder();
                if (!gamePath.Equals(string.Empty))
                {
                    this.IsSteam = false;
                    this.InstallFolder = gamePath;
                }
                else
                {
                    // game detection failed
                    // TODO: prompt user to manually select directory
                    this.IsSteam = true;
                    this.InstallFolder = string.Empty;
                }
            }
            else
            {
                this.IsSteam = true;
                this.InstallFolder = gamePath;
            }

            if (string.IsNullOrEmpty(this.InstallFolder))
            {
                MainWindow.ShowInitErrorAndExit();
            }

            // game directory is found, all good
            var edition = IsSteam ? "Steam" : "Rockstar Warehouse";
            Utils.Log($"Game found! Edition: {edition}");
            Utils.Log($"Installation folder: {InstallFolder}");

            var gtaFileVersionInfo = FileVersionInfo.GetVersionInfo(InstallFolder + @"\GTA5.exe");
            this.PatchVersion = gtaFileVersionInfo.ProductVersion;
            this.Language = gtaFileVersionInfo.Language;

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

            this.IsModded = Directory.Exists(this.InstallFolder) && Directory.Exists(this.InstallFolder + @"\mods") && Directory.GetFiles(this.InstallFolder + @"\mods").Length != 0;
            if (Directory.Exists(this.InstallFolder + Utils.MOD_FOLDER_ENDPOINT))
            {
                if (GetMods(false).Count > 0)
                {
                    this.IsModded = true;
                }
            }
            if (!IsModded && GetMods(false).Count != 0 || GetModdedRpfs().Count != 0)
            {
                this.IsModded = true;
            }
            this.GameProperties = GetGameProperties();

            s.Stop();
            Utils.Log("Patch version: " + PatchVersion);
            Utils.Log("Language: " + Language);
            Utils.Log("Enabled mods: " + GetMods(false).Count);
            Utils.Log("Total mods: " + GetMods(true).Count);
            Utils.Log("RPF mods: " + GetModdedRpfs().Count);
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
            var files = Directory.GetFiles(this.ModStorageFolder).ToList<string>();
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
                var mod = modsToEnable[i];
                var dst = string.Empty;
                switch (mod.Type)
                {
                    case Utils.ModType.NATIVE:
                    {
                        dst = InstallFolder + @"\" + mod.Filename;
                        break;
                    }
                    case Utils.ModType.SCRIPT:
                    {
                        dst = InstallFolder + Utils.SCRIPT_FOLDER_ENDPOINT + @"\" + mod.Filename;
                        break;
                    }
                }
                var src = ModStorageFolder + @"\" + modsToEnable[i].Filename;
                Utils.Log($"source: {src}{Environment.NewLine}destination: {dst}");
                File.Move(src, dst);
            }
        }

        public void EnableMod(Mod mod)
        {
            Utils.Log($"Enabling {mod.Filename}...");
            var src = ModStorageFolder + @"\" + mod.Filename;
            var dst = InstallFolder + @"\" + mod.Filename;
            Utils.Log($"source: {src}{Environment.NewLine}destination: {dst}");
            File.Move(src, dst);
        }

        public void DisableMods()
        {
            Utils.Log("Disabling all mods...");
            var mods = this.GetModFiles();
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
                var mod = modsToDisable[i];
                var src = string.Empty;
                switch (mod.Type)
                {
                    case Utils.ModType.NATIVE:
                    {
                        src = InstallFolder + @"\" + mod.Filename;
                        break;
                    }
                    case Utils.ModType.SCRIPT:
                    {
                        src = InstallFolder + Utils.SCRIPT_FOLDER_ENDPOINT + @"\" + mod.Filename;
                        break;
                    }
                }
                var dst = ModStorageFolder + @"\" + mod.Filename;
                Utils.Log($"source: {src}{Environment.NewLine}destination: {dst}");
                File.Move(src, dst);
            }
        }

        public void DisableMod(Mod mod)
        {
            Utils.Log($"Disabling {mod.Filename}...");
            var src = InstallFolder + @"\" + mod.Filename;
            var dst = ModStorageFolder + @"\" + mod.Filename;
            Utils.Log($"source: {src}{Environment.NewLine}destination: {dst}");
            File.Move(src, dst);
        }

        public List<Mod> GetMods(bool includeDisabled)
        {
            var mods = GetModFiles();
            mods.AddRange(GetScriptFiles());
            if (!includeDisabled) return mods;
            var disabledModFiles = Directory.GetFiles(this.ModStorageFolder);
            for (int i = 0; i < disabledModFiles.Length; i++)
            {
                var filename = disabledModFiles[i].Replace(this.ModStorageFolder, string.Empty);
                var type = filename.Contains(".dll") && !filename.ToLower().Contains(@"scripthookv") &&
                           !filename.ToLower().Contains(@"dsound") && !filename.ToLower().Contains(@"dinput8")
                    ? Utils.ModType.SCRIPT
                    : Utils.ModType.NATIVE;
                mods.Add(new Mod(disabledModFiles[i], type, ModStorageFolder, false));
            }

            return mods;
        }

        private List<Mod> GetModFiles()
        {
            var files = Directory.GetFiles(this.InstallFolder);
            var result = new List<Mod>();
            for (int i = 0; i < files.Length; i++)
            {
                var filename = files[i].Replace(this.InstallFolder + @"\", String.Empty);
                if (filename.ToLower().Contains(@".asi") || filename.ToLower().Contains(@"scripthookv.dll") || filename.ToLower().Contains("dsound") || filename.ToLower().Contains(@"dinput8.dll"))
                {
                    var mod = new Mod(files[i], Utils.ModType.NATIVE, this.InstallFolder, true);
                    result.Add(mod);
                }
            }
            return result;
        }

        private List<Mod> GetScriptFiles()
        {
            var res = new List<Mod>();
            var path = this.InstallFolder + Utils.SCRIPT_FOLDER_ENDPOINT + @"\";
            // scripts folder doesn't exist, therefore no scripts can be returned
            if (!Directory.Exists(path)) return res;

            // scripts folder exists
            var files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].EndsWith("dll") || files[i].EndsWith("cs") || files[i].EndsWith("lua"))
                {
                    var mod = new Mod(files[i].Replace(path, string.Empty), Utils.ModType.SCRIPT, true);
                    res.Add(mod);
                }
            }

            return res;
        }

        private List<Mod> GetModdedRpfs()
        {
            if (IsModded)
            {
                var moddedRpfPaths = Directory.GetFiles(this.InstallFolder + @"\mods");
                var moddedRpfList = new List<Mod>();
                for (int i = 0; i < moddedRpfPaths.Length; i++)
                {
                    var moddedRpf = new Mod(moddedRpfPaths[i], Utils.ModType.RPF, this.InstallFolder, true);
                    moddedRpfList.Add(moddedRpf);
                }
                return moddedRpfList;
            }
            else
            {
                return new List<Mod>();
            }
        }
        #endregion

        #region version management
        public bool IsScriptHookInstalled()
        {
            return File.Exists(this.InstallFolder + @"\ScriptHookV.dll");
        }

        public string GetScriptHookVersion()
        {
            return !IsScriptHookInstalled() ? string.Empty : FileVersionInfo.GetVersionInfo(this.InstallFolder + @"\ScriptHookV.dll").ProductVersion;
        }

        public bool IsScriptHookCompatible()
        {
            if (!IsScriptHookInstalled()) return true;

            var hookFilePath = this.InstallFolder + @"\ScriptHookV.dll";
            var hookVersion = Utils.ExtractVersion(FileVersionInfo.GetVersionInfo(hookFilePath).ProductVersion);
            return Utils.ExtractVersion(this.PatchVersion) <= hookVersion;
        }
        #endregion

        private List<GameProperty> GetGameProperties()
        {
            var properties = new List<GameProperty>
            {
                new GameProperty("Install folder", this.InstallFolder),
                new GameProperty("Patch version", this.PatchVersion),
                new GameProperty("Language", Language),
                new GameProperty("Edition", IsSteam ? "Steam" : "Rockstar Warehouse"),
                new GameProperty("Vanilla", IsModded ? "No" : "Yes")
            };
            return properties;
        }

        private static string GetRockstarGameInstallationFolder()
        {
            try
            {
                var installFolder = Registry.LocalMachine.OpenSubKey(GTA_REGISTRY_PATH).GetValue("InstallFolder")
                    .ToString();
                return installFolder;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error xdd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return string.Empty;
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
                    Utils.Log($"GameManager: GetSteamInstallationFolder -> {ex.Message}");
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private static string GetSteamClientInstallationFolder()
        {
            var steamKey32Bit = Registry.CurrentUser.OpenSubKey(STEAM_REGISTRY_PATH_x86);
            var steamKey64Bit = Registry.LocalMachine.OpenSubKey(STEAM_REGISTRY_PATH_x64);

            if (steamKey32Bit != null)
            {
                var path32 = steamKey32Bit.GetValue("SteamPath").ToString();
                return path32;
            }
            else if (steamKey64Bit != null)
            {
                var path64 = steamKey64Bit.GetValue("InstallPath").ToString();
                return path64;
            }
            else
            {
                Utils.Log($"GameManager: GetSteamClientInstallationFolder -> steam is not installed");
                MessageBox.Show("Steam does not appear to be installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            return string.Empty;
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
