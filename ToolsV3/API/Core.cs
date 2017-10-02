using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ToolsV3.API
{
    public static class Core
    {
        public static void Initialize()
        {
            Utils.Log("Core: initialize");
            Updater.Initialize();
        }

        public static void Save()
        {
            Utils.Log("Core: save properties");
            Properties.Settings.Default.Save();
        }
    }
}
