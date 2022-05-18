using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using CommandLine;
using CommandLine.Text;
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

            var parserResult = parser.ParseArguments<CliOptions>(args);

            parserResult.WithParsed(o =>
                            {
                                Settings = o;
                                new Program().Run();
                            })
                        .WithNotParsed(errs => DisplayHelp(parserResult, errs));
        }

        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> _)
        {
            var sb = new StringBuilder("List of available modules for --modules:").AppendLine();
            foreach (var module in ModuleFactory.ModuleMap)
            {
                sb.AppendLine($"\t{module.Key} -> {module.Value.Description}");
            }

            var helpText = HelpText.AutoBuild(result,
                    h =>
                    {
                        h.AdditionalNewLineAfterOption = false;
                        h.MaximumDisplayWidth = 120;
                        h.AddPostOptionsText(sb.ToString());

                        return HelpText.DefaultParsingErrorsHandler(result, h);
                    },
                    e => e
                );

            Console.Error.WriteLine(helpText);
        }

        private void Run()
        {
            using ISequenceReader? sequenceReader = ReaderFactory.Create(Settings);
            IEnumerable<IQcModule>? modules = ModuleFactory.Create(Settings);

            Console.Error.WriteLine($"Running modules:\n  {string.Join("\n  ", Settings.ModuleNames)}");

            On(Settings.ShowProgress, () => progressBar = new TimedSequenceProgressBar(sequenceReader));
            On(Settings.Verbose, () => Console.Error.WriteLine($"Processing {Settings.InputFilename}..."));

            while (sequenceReader.SequencesRead < Settings.ReadLimit && sequenceReader.ReadSequence(out Sequence? sequence))
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
                ["_metadata"] = new
                {
                    _modules = Settings.ModuleNames,
                    _inputFilename = Settings.InputFilename,
                    _outputFilename = string.IsNullOrWhiteSpace(Settings.OutputFilename) ? "STDOUT" : Settings.OutputFilename,
                    _sequences = sequenceReader.SequencesRead
                }
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
