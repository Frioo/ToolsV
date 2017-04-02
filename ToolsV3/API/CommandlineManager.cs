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
        }

        public List<string> GetCommandlineArguments()
        {
            List<string> res = new List<string>();
            string path = Manager.InstallFolder + @"/commandline.txt";
            if (!File.Exists(path))
            {
                File.Create(path);
                return res;
            }
            string[] commandlineContents = File.ReadAllLines(path);
            for (int i = 0; i < commandlineContents.Length; i++)
            {
                res.Add(commandlineContents[i]);
            }
            return res;
        }
    }
}
