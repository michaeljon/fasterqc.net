using System;
using System.Collections.Generic;
using System.Text.Json;

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
            // new BasicStatistics(),
            // new KMerContent(),
            // new NCountsAtPosition(),
            // new PerPositionSequenceContent(),
            // new PerSequenceGcContent(),
            new QualityDistributionByBase(),
            // new MeanQualityDistribution(),
            // new SequenceLengthDistribution(),
            // new PerPositionQuality()
        };

        static void Main(string[] args)
        {
            using var sequenceReader = new BamReader(args[0]);

            var sequencesProcessed = 0;

            while (sequenceReader.ReadSequence(out Sequence sequence))
            {
                foreach (var module in modules)
                {
                    module.ProcessSequence(sequence);
                }

                if (++sequencesProcessed % 100000 == 0)
                {
                    Console.Error.WriteLine($"{sequencesProcessed} sequences completed ~{sequenceReader.ApproximateCompletion()}%");
                }
            }

            Console.Error.WriteLine($"{sequencesProcessed} sequences processed");

            var results = new Dictionary<string, object>();
            foreach (var module in modules)
            {
                results[module.Name] = module.Data;
            }
            Console.WriteLine(JsonSerializer.Serialize(results, options));
        }
    }
}
