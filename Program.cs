using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommandLine;
using Ovation.FasterQC.Net.Modules;
using Ovation.FasterQC.Net.Readers;
using Ovation.FasterQC.Net.Utils;
using static Ovation.FasterQC.Net.Utils.CliOptions;

namespace Ovation.FasterQC.Net
{
    class Program
    {
        private static readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private TimedSequenceProgressBar? progressBar;

        static void Main(string[] args)
        {
            var parser = new Parser(config =>
                {
                    config.AutoHelp = true;
                    config.AutoVersion = true;
                    config.CaseInsensitiveEnumValues = true;
                }
            );

            parser.ParseArguments<CliOptions>(args)
                .WithParsed(o =>
                    {
                        Settings = o;
                        new Program().Run();
                    });
        }

        private void Run()
        {
            using var sequenceReader = ReaderFactory.Create(Settings);
            var modules = ModuleFactory.Create(Settings);

            Console.Error.WriteLine($"Running modules:\n  {string.Join("\n  ", Settings.ModuleNames)}");

            On(Settings.ShowProgress, () => progressBar = new TimedSequenceProgressBar(sequenceReader));
            On(Settings.Verbose, () => Console.Error.WriteLine($"Processing {Settings.InputFilename}..."));

            while (sequenceReader.ReadSequence(out Sequence? sequence) && sequenceReader.SequencesRead < Settings.ReadLimit)
            {
                ArgumentNullException.ThrowIfNull(sequence);

                foreach (var module in modules)
                {
                    module.ProcessSequence(sequence);
                }

                On(Settings.ShowProgress, () => progressBar?.Update());
                On(Settings.Verbose, () =>
                {
                    if (sequenceReader.SequencesRead % UpdatePeriod == 0)
                    {
                        var approximateCompletion = sequenceReader.ApproximateCompletion;

                        // if we're limiting the number of reads then the reader's
                        // approximation will be incorrect (it's based on file positions),
                        // so we'll do the math ourselves
                        if (Settings.ReadLimit < ulong.MaxValue)
                        {
                            approximateCompletion = 100.0 * (double)sequenceReader.SequencesRead / (double)Settings.ReadLimit;
                        }

                        Console.Error.WriteLine($"{sequenceReader.SequencesRead.WithSsiUnits()} sequences completed ({approximateCompletion:0.0}%)");
                    }
                });
            }

            var results = new Dictionary<string, object>()
            {
                ["_modules"] = Settings.ModuleNames,
                ["_inputFilename"] = Settings.InputFilename,
                ["_outputFilename"] = string.IsNullOrWhiteSpace(Settings.OutputFilename) ? "STDOUT" : Settings.OutputFilename,
            };

            foreach (var module in modules)
            {
                results[module.Name] = module.Data;
            }

            On(Settings.ShowProgress, () => progressBar?.Update(force: true));
            On(Settings.Verbose, () => Console.Error.WriteLine($"{sequenceReader.SequencesRead.WithSsiUnits()} sequences completed ({sequenceReader.ApproximateCompletion:0.0}%)"));

            if (string.IsNullOrWhiteSpace(Settings.OutputFilename))
            {
                Console.WriteLine(JsonSerializer.Serialize(results, options));
            }
            else
            {
                File.WriteAllText(Settings.OutputFilename, JsonSerializer.Serialize(results, options));
            }
        }
    }
}
