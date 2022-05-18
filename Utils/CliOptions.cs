using System;
using System.Collections.Generic;
using System.Globalization;
using CommandLine;

namespace Ovation.FasterQC.Net.Utils
{
    public class CliOptions
    {
        [Option('v', "verbose", Required = false, SetName = "Verbose", HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option("debug", Required = false, SetName = "Verbose", HelpText = "Show diagnostic output.  Can only use with --verbose.")]
        public bool Debug { get; set; }

        [Option('p', "progress", Required = false, SetName = "Progress", HelpText = "Show progress bar.  Cannnot use with --verbose.")]
        public bool ShowProgress { get; set; }

        [Option('i', "input", Required = true, HelpText = "Input filename.")]
        public string InputFilename { get; set; } = null!;

        [Option('o', "output", Required = false, HelpText = "Output filename.  Defaults to STDOUT.")]
        public string OutputFilename { get; set; } = null!;

        [Option('b', "bam", Required = false, HelpText = "Assume BAM format.")]
        public bool Bam { get; set; }

        [Option('f', "fastq", Required = false, HelpText = "Assume FASTQ format.")]
        public bool Fastq { get; set; }

        [Option('z', "zipped", Required = false, HelpText = "Assume input file is gzipped.")]
        public bool Zipped { get; set; }

        [Option('m', "modules", Required = true, Min = 1, HelpText = "Space-separated list of modules to run, or 'all'.")]
        public IEnumerable<string> ModuleNames { get; set; } = Array.Empty<string>();

        public static CliOptions Settings { get; set; } = null!;

        public const int UpdatePeriod = 100_000;

        public static void On(bool condition, Action action)
        {
            if (condition)
            {
                action();
            }
        }

        public bool Validate()
        {
            if (!(Bam || Fastq))
            {
                Fastq = InputFilename.EndsWith(".fastq", ignoreCase: true, culture: CultureInfo.InvariantCulture)
                        || InputFilename.EndsWith(".fastq.gz", ignoreCase: true, culture: CultureInfo.InvariantCulture);

                Bam = InputFilename.EndsWith(".bam", ignoreCase: true, culture: CultureInfo.InvariantCulture)
                        || InputFilename.EndsWith(".bam.gz", ignoreCase: true, culture: CultureInfo.InvariantCulture);
            }

            if (Zipped == false && !string.IsNullOrWhiteSpace(InputFilename))
            {
                if (InputFilename.EndsWith(".gz", ignoreCase: true, culture: CultureInfo.InvariantCulture))
                {
                    Zipped = true;
                }
            }

            return Fastq || Bam;
        }
    }
}
