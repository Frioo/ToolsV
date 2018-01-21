using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string oldFileName = options.OldFilePath.Replace(currentDir + @"\", string.Empty)
                    .Replace(@".exe", string.Empty);
                string newFileName = options.NewFilePath.Replace(currentDir + @"\", string.Empty);
                Console.WriteLine($"Current path: {currentDir}");
                KillProcessByName(@"ToolsV3", options.OldFilePath);
                try
                {
                    Console.WriteLine("Deleting old executable...");
                    File.SetAttributes(options.OldFilePath, FileAttributes.Normal);
                    File.Delete(options.OldFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                Console.WriteLine($"Launching updated program: {newFileName}");
                Process.Start(options.NewFilePath);
            }
            Console.ReadKey(false);
            Environment.Exit(0);
        }

        private static void KillProcess(string filename)
        {
            var spi = new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/f /im {filename}",
                UseShellExecute = true,
                CreateNoWindow = true
            };
            Process.Start(spi)?.WaitForExit();
        }

        private static void KillProcessByName(string processName, string filePath)
        {
            Console.WriteLine($"Looking for process: {processName}");
            var results = Process.GetProcessesByName(processName);
            if (results.Length < 1) return;
            foreach (var proc in results)
            {
                if (!proc.MainModule.FileName.Equals(filePath)) continue;
                Console.WriteLine($"Killing process {proc.ProcessName}...");
                proc.Kill();
            }
        }
    }
}
