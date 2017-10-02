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
        public bool IsEnabled { get; set; }

        public Mod(string filename, bool enabled)
        {
            this.Filename = filename;
            this.IsEnabled = enabled;
        }

        public Mod(string filePath, string installPath, bool enabled)
        {
            this.Filename = filePath.Replace(installPath + @"\", String.Empty);
            this.IsEnabled = enabled;
        }
    }
}
