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
    internal class Program : IDisposable
    {
        private static readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private TimedSequenceProgressBar? progressBar;

        private static void Main(string[] args)
        {
            Parser? parser = new(config =>
                {
                    config.AutoHelp = true;
                    config.AutoVersion = true;
                    config.CaseInsensitiveEnumValues = true;
                }
            );

            _ = parser.ParseArguments<CliOptions>(args)
                .WithParsed(o =>
                    {
                        Settings = o;
                        new Program().Run();
                    });
        }

        private void Run()
        {
            using ISequenceReader? sequenceReader = ReaderFactory.Create(Settings);
            IEnumerable<IQcModule>? modules = ModuleFactory.Create(Settings);

            Console.Error.WriteLine($"Running modules:\n  {string.Join("\n  ", Settings.ModuleNames)}");

            On(Settings.ShowProgress, () => progressBar = new TimedSequenceProgressBar(sequenceReader));
            On(Settings.Verbose, () => Console.Error.WriteLine($"Processing {Settings.InputFilename}..."));

            while (sequenceReader.ReadSequence(out Sequence? sequence) && sequenceReader.SequencesRead < Settings.ReadLimit)
            {
                ArgumentNullException.ThrowIfNull(sequence);

                foreach (IQcModule? module in modules)
                {
                    module.ProcessSequence(sequence);
                }

                On(Settings.ShowProgress, () => progressBar?.Update());
                On(Settings.Verbose, () =>
                {
                    if (sequenceReader.SequencesRead % UpdatePeriod == 0)
                    {
                        WritePercentComplete(sequenceReader.ApproximateCompletion, sequenceReader.SequencesRead);
                    }
                });
            }

            Dictionary<string, object>? results = new()
            {
                ["_modules"] = Settings.ModuleNames,
                ["_inputFilename"] = Settings.InputFilename,
                ["_outputFilename"] = string.IsNullOrWhiteSpace(Settings.OutputFilename) ? "STDOUT" : Settings.OutputFilename,
            };

            foreach (IQcModule? module in modules)
            {
                results[module.Name] = module.Data;
            }

            On(Settings.ShowProgress, () => progressBar?.Update(force: true));
            On(Settings.Verbose, () => WritePercentComplete(sequenceReader.ApproximateCompletion, sequenceReader.SequencesRead));

            if (string.IsNullOrWhiteSpace(Settings.OutputFilename))
            {
                Console.WriteLine(JsonSerializer.Serialize(results, options));
            }
            else
            {
                File.WriteAllText(Settings.OutputFilename, JsonSerializer.Serialize(results, options));
            }
        }

        private static void WritePercentComplete(double reportedCompletion, ulong sequencesRead)
        {
            double approximateCompletion = reportedCompletion;

            // if we're limiting the number of reads then the reader's
            // approximation will be incorrect (it's based on file positions),
            // so we'll do the math ourselves
            if (Settings.ReadLimit < ulong.MaxValue)
            {
                approximateCompletion = 100.0 * sequencesRead / Settings.ReadLimit;
            }

            Console.Error.WriteLine($"{sequencesRead.WithSsiUnits()} sequences completed ({approximateCompletion:0.0}%)");
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
