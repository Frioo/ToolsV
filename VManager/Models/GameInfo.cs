using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsV.Models
{
    public class GameInfo
    {
        public string? Version { get; set; }
        public string? Path { get; set; }
        public string? Language { get; set; }
        public GameEdition Edition { get; set; }
        public bool IsVanilla { get; set; }
    }

    public enum GameEdition
    {
        STEAM = 0,
        ROCKSTAR = 1,
        EPIC = 2,
        UNKNOWN = 3,
    }
}
