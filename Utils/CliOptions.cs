using System;
using System.Collections.Generic;
using CommandLine;

namespace Ovation.FasterQC.Net.Utils
{
    public class CliOptions
    {
        [Option('v', "verbose", Required = false, SetName = "Verbose", HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('d', "debug", Required = false, SetName = "Verbose", HelpText = "Show diagnostic output.  Can only use with --verbose.")]
        public bool Debug { get; set; }

        [Option('p', "progress", Required = false, SetName = "Progress", HelpText = "Show progress bar.  Cannnot use with --verbose.")]
        public bool ShowProgress { get; set; }

        [Option('i', "input", Required = true, HelpText = "Input filename.")]
        public string InputFilename { get; set; } = null!;

        [Option('o', "output", Required = false, HelpText = "Output filename.  Defaults to STDOUT.")]
        public string OutputFilename { get; set; } = null!;

        [Option('f', "format", Required = true, HelpText = "Type of input file.")]
        public ReaderType Format { get; set; }

        [Option('m', "modules", Required = false, Default = new string[] { "all" }, HelpText = "Space-separated list of modules to run, or 'all'.")]
        public IEnumerable<string> ModuleNames { get; set; } = Array.Empty<string>();

        [Option('l', "read-limit", Required = false, HelpText = "Limit the number of reads processed.")]
        public ulong ReadLimit { get; set; } = ulong.MaxValue;

        [Option('h', "html", Required = false, HelpText = "Write self-contained HTML output to specified filename.")]
        public string HtmlOut { get; set; } = null!;

        public static CliOptions Settings { get; set; } = null!;

        public const int UpdatePeriod = 100_000;

        public static void On(bool condition, Action action)
        {
            if (condition)
            {
                action();
            }
        }
    }
}
