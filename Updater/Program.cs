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
                string oldFileName = options.OldFilePath.Replace(currentDir + @"\", String.Empty);
                string newFileName = options.NewFilePath.Replace(currentDir + @"\", String.Empty);
                Console.WriteLine($"Current path: {currentDir}");
                Console.WriteLine($"Attempting to kill process: {oldFileName}");
                KillProcess(oldFileName);
                try
                {
                    Console.WriteLine("Deleting old executable...");
                    File.SetAttributes(options.OldFilePath, FileAttributes.Normal);
                    File.Delete(options.OldFilePath);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                Console.WriteLine($"Launching updated program: {newFileName}");
                Process.Start(options.NewFilePath);
            }
            Environment.Exit(0);
        }

        private static void KillProcess(string filename)
        {
            ProcessStartInfo spi = new ProcessStartInfo();
            spi.FileName = "taskkill";
            spi.Arguments = $"/f /im {filename}";
            spi.UseShellExecute = true;
            spi.CreateNoWindow = true;

            Process.Start(spi);
        }
    }
}
