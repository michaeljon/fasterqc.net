using System;
using System.Linq;

namespace Ovation.FasterQC.Net
{
    public class PerSequenceGcContent : IQcModule
    {
        private ulong[] gcCounts = Array.Empty<ulong>();
        private ulong sequenceCount;

        public string Name => "gcDistribution";

        public string Description => "Distribution of GC content percentages";

        public object Data => gcCounts.Select(a => Math.Round((double)a / (double)sequenceCount * 100.0, 3));

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;
            sequenceCount++;

            // see if we need to resize this
            if (sequenceLength > gcCounts.Length)
            {
                Array.Resize(ref gcCounts, sequenceLength);
            }

            var chars = sequence.Read.ToArray();
            for (var c = 0; c < chars.Length; c++)
            {
                if (chars[c] == 'G' || chars[c] == 'C')
                {
                    gcCounts[c]++;
                }
            }
        }

        public void Reset()
        {
            gcCounts = Array.Empty<ulong>();
        }
    }
}
