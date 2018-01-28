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
        public Utils.ModType Type { get; set; }
        public bool IsEnabled { get; set; }

        public Mod(string filename, Utils.ModType type, bool enabled)
        {
            this.Filename = filename;
            this.Type = type;
            this.IsEnabled = enabled;
        }

        public Mod(string filePath, Utils.ModType type, string installPath, bool enabled)
        {
            this.Filename = filePath.Replace(installPath + @"\", String.Empty);
            this.Type = type;
            this.IsEnabled = enabled;
        }
    }
}
