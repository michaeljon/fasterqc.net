using System;
using System.Linq;

namespace Ovation.FasterQC.Net
{
    public class PerPositionSequenceContent : IQcModule
    {
        private int[] aCounts = Array.Empty<int>();
        private int[] cCounts = Array.Empty<int>();
        private int[] tCounts = Array.Empty<int>();
        private int[] gCounts = Array.Empty<int>();
        private long sequenceCount;

        public string Name => "baseCounts";

        public string Description => "Calculates ATCG counts at position along sequence";

        public object Data
        {
            get
            {
                var aPercentage = aCounts.Select(a => Math.Round((double)a / (double)sequenceCount * 100.0, 3));
                var cPercentage = aCounts.Select(c => Math.Round((double)c / (double)sequenceCount * 100.0, 3));
                var tPercentage = aCounts.Select(t => Math.Round((double)t / (double)sequenceCount * 100.0, 3));
                var gPercentage = aCounts.Select(g => Math.Round((double)g / (double)sequenceCount * 100.0, 3));

                return new
                {
                    aPercentage,
                    cPercentage,
                    tPercentage,
                    gPercentage
                };
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;
            sequenceCount++;

            // see if we need to resize this
            if (sequenceLength > aCounts.Length)
            {
                Array.Resize(ref aCounts, sequenceLength);
                Array.Resize(ref cCounts, sequenceLength);
                Array.Resize(ref tCounts, sequenceLength);
                Array.Resize(ref gCounts, sequenceLength);
            }

            var chars = sequence.Read.ToArray();
            for (var c = 0; c < chars.Length; c++)
            {
                switch (chars[c])
                {
                    case (byte)'A': aCounts[c]++; break;
                    case (byte)'C': cCounts[c]++; break;
                    case (byte)'T': tCounts[c]++; break;
                    case (byte)'G': gCounts[c]++; break;
                }
            }
        }

        public void Reset()
        {
            aCounts = Array.Empty<int>();
            cCounts = Array.Empty<int>();
            tCounts = Array.Empty<int>();
            gCounts = Array.Empty<int>();
            sequenceCount = 0;
        }
    }
}