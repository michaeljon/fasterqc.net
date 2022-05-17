using System;
using System.Collections.Generic;
using System.IO;
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
            Parser.Default.ParseArguments<CliOptions>(args)
                .WithParsed(o =>
                {
                    o.Validate();
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

            while (sequenceReader.ReadSequence(out Sequence? sequence))
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
                        Console.Error.WriteLine($"{sequenceReader.SequencesRead.WithSsiUnits()} sequences completed ({sequenceReader.ApproximateCompletion:0.0}%)");
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
