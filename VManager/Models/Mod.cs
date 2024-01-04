using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsV.Models
{
    public class Mod
    {
        public string Filename => Path.Split(@"\").Last();
        public string Path { get; set; }
        public ModType Type { get; set; }
        public bool IsEnabled { get; set; }
    }

    public enum ModType
    {
        NATIVE = 1,
        SCRIPT = 2,
        RPF = 3
    }
}
