using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using ToolsV.Logic.FindGame;
using ToolsV.Models;

namespace ToolsV
{
    public class VManager
    {
        public GameInfo GameInfo { get; private set; }

        private VManager()
        {

        }

        public static VManager Init()
        {
            var gameInfo = GetGameInfo();
            
            if (gameInfo == null)
            {
                throw new Exception("Failed to find GTA V installation directory.");
            }

            var instance = new VManager()
            {
                GameInfo = gameInfo
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

            return res;
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
    }
}
