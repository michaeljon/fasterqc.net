using System;
using System.Collections.Generic;

namespace Ovation.FasterQC.Net
{
    public class SequenceLengthDistribution : IQcModule
    {
        private int minimumReadLength = int.MaxValue;
        private int maximumReadLength = int.MinValue;

        private static readonly IDictionary<int, long> lengths = new Dictionary<int, long>();

        public string Name => "sequenceLengthDistribution";

        public string Description => "Calculates the sequence length distributions";

        public object Data
        {
            get
            {
                var results = new long[maximumReadLength + 1];

                foreach (var (length, count) in lengths)
                {
                    results[length] = count;
                }

                return new
                {
                    minimumReadLength,
                    maximumReadLength,
                    results
                };
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;

            minimumReadLength = Math.Min(minimumReadLength, sequenceLength);
            maximumReadLength = Math.Max(maximumReadLength, sequenceLength);

            if (lengths.ContainsKey(sequenceLength) == false)
            {
                lengths.Add(sequenceLength, 0);
            }

            lengths[sequenceLength]++;
        }

        public void Reset()
        {
            minimumReadLength = int.MaxValue;
            maximumReadLength = int.MinValue;

            lengths.Clear();
        }
    }
}