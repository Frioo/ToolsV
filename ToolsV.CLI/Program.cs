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
            }

            Console.WriteLine($"Patch: {manager?.GameInfo.Version}");
            Console.WriteLine($"Installed at: {manager?.GameInfo.Path}");
            Console.WriteLine($"Edition: {manager?.GameInfo.Edition}");
        }
    }
}
