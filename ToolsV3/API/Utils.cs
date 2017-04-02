using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsV3
{
    public static class Utils
    {
        private static string TAG = @"ToolsV:Log -> ";

        public enum LaunchMode
        {
            NORMAL = 1,
            SINGLEPLAYER_WITH_MODS = 2,
            SINGLEPLAYER_WITHOUT_MODS = 3,
            ONLINE = 4
        }

        public static void Log(string text)
        {
            Debug.WriteLine(TAG + text);
        }
    }
}
