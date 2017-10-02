using CommandLine;
using System;

namespace Updater
{
    class Options
    {
        [Option('o', "old", HelpText = "Path to previous release (executable).", Required = true)]
        public string OldFilePath { get; set; }

        [Option('n', "new", HelpText = "Path to the latest release (executable).", Required = true)]
        public string NewFilePath { get; set; }
    }
}
