using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsV.Models;

namespace VManager.Logic.ModManager
{
    public class ModManager
    {
        private GameInfo _gameInfo;
        private string _gamePath => _gameInfo.Path;

        public string ScriptsPath => Path.Combine(_gamePath, "scripts");
        public string RpfPath => Path.Combine(_gamePath, "mods");

        public ModManager(GameInfo gameInfo)
        {
            _gameInfo = gameInfo;
        }

        public List<Mod> GetMods(bool includeDisabled = false)
        {
            var res = new List<Mod>();

            res.AddRange(GetModFiles());
            res.AddRange(GetScriptMods());
            res.AddRange(GetRpfMods());

            return res;
        }

        private List<Mod> GetModFiles()
        {
            var files = Directory.GetFiles(_gamePath);
            var res = new List<Mod>();
            for (int i = 0; i < files.Length; i++)
            {
                var filename = files[i].Replace(_gamePath + @"\", String.Empty);
                if (filename.ToLower().Contains(@".asi") || 
                    filename.ToLower().Contains(@"scripthookv.dll") || 
                    filename.ToLower().Contains("dsound") || 
                    filename.ToLower().Contains(@"dinput8.dll"))
                {
                    res.Add(new Mod
                    {
                        Path = filename,
                        Type = ModType.NATIVE,
                        IsEnabled = true,
                    });
                }
            }
            return res;
        }

        private List<Mod> GetScriptMods()
        {
            var res = new List<Mod>();

            // scripts folder doesn't exist, return empty list
            if (!Directory.Exists(ScriptsPath)) return res;

            // scripts folder exists
            var files = Directory.GetFiles(ScriptsPath);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].EndsWith("dll") || files[i].EndsWith("cs") || files[i].EndsWith("lua"))
                {
                    res.Add(new Mod
                    {
                        Path = files[i],
                        Type = ModType.SCRIPT,
                        IsEnabled = true,
                    });
                }
            }

            return res;
        }

        private List<Mod> GetRpfMods()
        {
            var res = new List<Mod>();

            // mods folder doesn't exist, return empty list
            if (!Directory.Exists(RpfPath)) return res;

            var files = Directory.GetFiles(RpfPath);
            for (int i = 0; i < files.Length; i++)
            {
                res.Add(new Mod
                {
                    Path = files[i],
                    Type = ModType.RPF,
                    IsEnabled = true,
                });
            }

            return res;
        }
    }
}
