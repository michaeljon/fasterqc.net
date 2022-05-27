using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Ovation.FasterQC.Net.Utils;
using static Ovation.FasterQC.Net.Utils.CliOptions;

namespace Ovation.FasterQC.Net
{
    public class AdapterContent : IQcModule
    {
        private readonly List<Adapter> adapters = null!;
        private long totalCount;
        public bool calculated;
        private int longestSequence;
        private int longestAdapter;
        private double[][] enrichments = null!;
        private string[] xLabels = null!;
        BaseGroup[] groups = null!;

        public string Name => "adapterContent";

        public string Description => "Searches for specific adapter sequences in a library";

        public bool IsEnabledForAll => true;

        public AdapterContent()
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("configuration/adapter_list.jsonc"), new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            }) ?? throw new ArgumentException("cannot locate configuration/adapter_list.jsonc");
            adapters = dict.Select(kvp => new Adapter(kvp.Key, kvp.Value)).ToList();
        }

        public void ProcessSequence(Sequence sequence)
        {
            calculated = false;
            ++totalCount;

            // We need to be careful about making sure that a sequence is not only longer
            // than we've seen before, but also that the last position we could find a hit
            // is a positive position.

            // If the sequence is longer than it was then we need to expand the storage in
            // all of the adapter objects to account for this.

            if (sequence.Read.Length > longestSequence && sequence.Read.Length - longestAdapter > 0)
            {
                longestSequence = sequence.Read.Length;
                for (int a = 0; a < adapters.Count; a++)
                {
                    adapters[a].ExpandLengthTo(longestSequence - longestAdapter + 1);
                }
            }

            for (int a = 0; a < adapters.Count; a++)
            {
                int index = Encoding.ASCII.GetString(sequence.Read).IndexOf(adapters[a].Sequence);
                if (index >= 0)
                {
                    for (int i = index; i <= longestSequence - longestAdapter; i++)
                    {
                        adapters[a].IncrementCount(i);
                    }
                }
            }
        }

        public void Reset()
        {
            totalCount = longestSequence = longestAdapter = 0;
            calculated = false;
        }

        private double[][] CalculateEnrichments()
        {
            int maxLength = 0;
            for (int a = 0; a < adapters.Count; a++)
            {
                if (adapters[a].Positions.Length > maxLength)
                {
                    maxLength = adapters[a].Positions.Length;
                }
            }

            // We'll be grouping together positions later so make up the groups now
            groups = BaseGroup.MakeBaseGroups(maxLength);

            On(Settings.Debug, () => Console.Error.WriteLine($"Made {groups.Length} groups from {maxLength}"));

            xLabels = new string[groups.Length];
            for (int i = 0; i < xLabels.Length; i++)
            {
                xLabels[i] = groups[i].ToString();
            }

            enrichments = new double[adapters.Count][].FullyAllocate(groups.Length);

            for (int a = 0; a < adapters.Count; a++)
            {
                long[] positions = adapters[a].Positions;

                for (int g = 0; g < groups.Length; g++)
                {
                    On(Settings.Debug, () => Console.Error.WriteLine($"Looking at group {groups[g]}"));

                    for (int p = groups[g].LowerCount - 1; p < groups[g].UpperCount && p < positions.Length; p++)
                    {
                        On(Settings.Debug, () => Console.Error.WriteLine($"Count at position {p} is {positions[p]}"));
                        enrichments[a][g] += positions[p] * 100d / totalCount;
                        On(Settings.Debug, () => Console.Error.WriteLine($"Percentage at position {p} is {positions[p] * 100d / totalCount} total count is {totalCount}"));
                    }

                    enrichments[a][g] /= groups[g].UpperCount - groups[g].LowerCount + 1;
                    On(Settings.Debug, () => Console.Error.WriteLine($"Averge Percetage for group {groups[g]} is {enrichments[a][g]}"));
                }
            }

            calculated = true;

            return enrichments;
        }

        public object Data => new
        {
            platforms = adapters.Select(a => a.Name),
            data = calculated ? enrichments : CalculateEnrichments(),
            xLabels,
        };
    }
}
