using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsV.Models;

namespace ToolsV.Logic.FindGame
{
    public class FindGameRockstarStrategy : IFindGameStrategy
    {
        private static string RG_GTA_REGISTRY_PATH = @"SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V";

        public string? FindGameDirectory()
        {
            var installFolder = Registry.LocalMachine
                .OpenSubKey(RG_GTA_REGISTRY_PATH)?
                .GetValue("InstallFolder")?
                .ToString();

            return installFolder;
        }
    }
}
