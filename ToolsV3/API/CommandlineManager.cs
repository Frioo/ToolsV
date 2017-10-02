using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsV3.API
{
    public class CommandlineManager
    {
        private GameManager Manager { get; }

        public CommandlineManager(GameManager manager)
        {
            this.Manager = manager;
            RemoveMultiplayerFlag();
        }

        public void RemoveMultiplayerFlag()
        {
            RemoveCommandlineArgument("-goStraightToMP");
        }

        public void SetCommandLineArgument(Flag flag)
        {
            File.AppendAllLines(Manager.CommandlinePath, new List<string> { flag.FlagCode }.AsEnumerable());
        }

        public void SetCommandlineArguments(List<Flag> enabledFlags)
        {
            List<string> flags = new List<string>();
            for (int i = 0; i < enabledFlags.Count; i++)
            {
                flags.Add(enabledFlags[i].FlagCode);
            }
            File.WriteAllText(Manager.CommandlinePath, String.Empty);
            File.AppendAllLines(Manager.CommandlinePath, flags.AsEnumerable());
        }

        public void RemoveCommandlineArgument(string flagCode)
        {
            List<Flag> flags = GetCommandlineArguments();
            for (int i = 0; i < flags.Count; i++)
            {
                if (flags[i].FlagCode.Equals(flagCode))
                {
                    flags.RemoveAt(i);
                }
            }
            SetCommandlineArguments(flags);
        }

        public List<Flag> GetCommandlineArguments()
        {
            List<Flag> res = new List<Flag>();
            List<Flag> all = this.GetAllFlags();
            string path = Manager.InstallFolder + @"\commandline.txt";
            if (!File.Exists(path))
            {
                File.Create(path);
                return res;
            }


            List<string> args = File.ReadAllLines(path).ToList();
            foreach (string arg in args)
            {
                var query = from element in all
                            where element.FlagCode.Equals(arg)
                            select element;

                foreach (var f in query)
                {
                    f.IsEnabled = true;
                    res.Add(f);
                    Utils.Log(f.FlagCode);
                }
            }
            return res;
        }

        public List<Flag> GetAllFlags()
        {
            List<Flag> flags = new List<Flag>();
            //flags.Add(new Flag("-verify", "verifies game files integrity and checks for updates"));
            //flags.Add(new Flag("-safemode", "starts the game with minimal settings but doesn't save them"));
            flags.Add(new Flag("-ignoreprofile", "ignores current profile settings"));
            flags.Add(new Flag("-useMinimumSettings", "starts the game with minimal settings"));
            flags.Add(new Flag("-useAutoSettings", "game uses automatic settings"));
            flags.Add(new Flag("-DX10", "forces DirectX 10.0"));
            flags.Add(new Flag("-DX10_1", "forces DirectX 10.1"));
            flags.Add(new Flag("-DX11", "forces DirectX 11.0"));
            flags.Add(new Flag("-noChunkedDownload", "forces downloading all updates at once instead of parts"));
            flags.Add(new Flag("-benchmark", "runs a system performance test"));
            flags.Add(new Flag("-goStraightToMP", "automatically loads online mode"));
            //flags.Add(new Flag("-StraightIntoFreemode", "load GTA online freemode"));
            flags.Add(new Flag("-windowed", "forces the game to run in a window"));
            flags.Add(new Flag("-fullscreen", "forces fullscreen mode"));
            flags.Add(new Flag("-borderless", "hides window borders"));
            flags.Add(new Flag("-disallowResizeWindow", "locks window size"));
            // TODO: add language switcher

            return flags;
        }
    }
}
