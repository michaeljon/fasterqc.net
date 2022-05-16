using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CommandLine;
using fasterqc.net.Readers;
using fasterqc.net.Utils;
using static fasterqc.net.Utils.CliOptions;

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

        private static readonly List<IQcModule> modules = new()
        {
            new BasicStatistics(),
            // new KMerContent(),
            // new NCountsAtPosition(),
            // new PerPositionSequenceContent(),
            // new PerSequenceGcContent(),
            // new QualityDistributionByBase(),
            // new MeanQualityDistribution(),
            // new SequenceLengthDistribution(),
            // new PerPositionQuality()
        };

        private TimedSequenceProgressBar progressBar;

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

            On(Settings.ShowProgress, () => progressBar = new TimedSequenceProgressBar(sequenceReader));
            On(Settings.Verbose, () => Console.Error.WriteLine($"Processing {Settings.InputFilename}..."));

            while (sequenceReader.ReadSequence(out Sequence sequence))
            {
                foreach (var module in modules)
                {
                    module.ProcessSequence(sequence);
                }

                On(Settings.ShowProgress, () => progressBar.Update());
                On(Settings.Verbose, () =>
                {
                    if (sequenceReader.SequencesRead % UpdatePeriod == 0)
                    {
                        Console.Error.WriteLine($"{sequenceReader.SequencesRead.WithSsiUnits()} sequences completed ({sequenceReader.ApproximateCompletion:0.0}%)");
                    }
                });
            }

            var results = new Dictionary<string, object>();
            foreach (var module in modules)
            {
                results[module.Name] = module.Data;
            }

            On(Settings.ShowProgress, () => progressBar.Update(force: true));
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
