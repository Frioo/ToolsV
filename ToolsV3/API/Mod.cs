using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsV3
{
    public class Mod
    {
        public string Filename { get; }
        public string Path { get; }

        public Mod(string path, string gamePath)
        {
            this.Filename = path.Replace(gamePath, String.Empty);
            this.Path = path;
        }
    }
}
