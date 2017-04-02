using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsV3.API
{
    public class Flag
    {
        public string FlagCode { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }

        public Flag(string code, string description)
        {
            this.FlagCode = code;
            this.Description = description;
            this.Enabled = false;
        }
    }
}
