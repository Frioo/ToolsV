using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using ToolsV.Logic.FindGame;
using ToolsV.Models;
using VManager.Logic.ModManager;
using VManager.Resources;

namespace ToolsV
{
    public class VManager
    {
        public GameInfo GameInfo { get; private set; }
        public ModManager ModManager { get; private set; }

        private VManager()
        {

        }

        public static VManager Init()
        {
            var gameInfo = GetGameInfo();
            
            if (gameInfo == null)
            {
                throw new Exception(Common.FailedToDetectGameInstallation);
            }

            var instance = new VManager()
            {
                GameInfo = gameInfo,
                ModManager = new ModManager(gameInfo),
            };

            return instance;
        }

        public static GameInfo? GetGameInfo()
        {
            var res = GetGameDirectory();

            if (res == null || string.IsNullOrWhiteSpace(res.Path))
            {
                return null;
            }

            var executablePath = Path.Combine(res.Path, "GTA5.exe");
            var fileInfo = FileVersionInfo.GetVersionInfo(executablePath);

            res.Version = fileInfo.ProductVersion;
            res.Language = fileInfo.Language;
            res.IsWriteProtected = GetGameWriteProtection(res.Path);

            return res;
        }

        public static bool GetGameWriteProtection(string dir)
        {
            var dirInfo = new DirectoryInfo(dir);

            var accessRules = dirInfo
                .GetAccessControl()
                .GetAccessRules(true, true, typeof(NTAccount));

            var isReadOnly = dirInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
            var isWriteProtected = true;
            
            foreach (FileSystemAccessRule rule in accessRules)
            {
                if (rule.FileSystemRights == FileSystemRights.Write)
                {
                    isWriteProtected = false;
                }
            }

            return isReadOnly || isWriteProtected;
        }

        public static GameInfo? GetGameDirectory()
        {
            var path = new FindGameRockstarStrategy().FindGameDirectory();

            if (!string.IsNullOrEmpty(path))
            {
                return new GameInfo
                {
                    Path = path,
                    Edition = GameEdition.ROCKSTAR
                };
            }

            path = new FindGameSteamStrategy().FindGameDirectory();

            if (!string.IsNullOrEmpty(path))
            {
                return new GameInfo
                {
                    Path = path,
                    Edition = GameEdition.STEAM,
                };
            }

            path = new FindGameEpicStrategy().FindGameDirectory();

            if (!string.IsNullOrEmpty(path))
            {
                return new GameInfo
                {
                    Path = path,
                    Edition = GameEdition.EPIC,
                };
            }

            return null;
        }

        /**
         * Mod Management
         */
    }
}
