using ToolsV;

namespace ToolsV.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ToolsV 4");
            
            var manager = VManager.Init();

            if (manager == null)
            {
                Console.WriteLine("GTA V installation directory was not found. You might be missing some registry keys.");
                return;
            }

            Console.WriteLine($"Patch: {manager?.GameInfo.Version}");
            Console.WriteLine($"Installed at: {manager?.GameInfo.Path}");
            Console.WriteLine($"Edition: {manager?.GameInfo.Edition}");
            Console.WriteLine($"Language: {manager?.GameInfo.Language}");
            Console.WriteLine($"Write-protected: " + (manager?.GameInfo.IsWriteProtected == true ? "Yes" : "No"));

            var mods = manager.ModManager.GetMods();
            Console.WriteLine($"Mods: {mods.Count}");
            foreach (var mod in mods)
            {
                Console.WriteLine($"- {mod.Filename} ({mod.Type})");
            }
        }
    }
}
