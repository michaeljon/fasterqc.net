using System;
using System.Linq;

namespace Ovation.FasterQC.Net
{
    public class PerPositionSequenceContent : IQcModule
    {
        private ulong[] aCounts = Array.Empty<ulong>();

        private ulong[] cCounts = Array.Empty<ulong>();

        private ulong[] tCounts = Array.Empty<ulong>();

        private ulong[] gCounts = Array.Empty<ulong>();
        private ulong sequenceCount;

        public string Name => "baseCounts";

        public string Description => "Calculates ATCG counts at position along sequence";

        public ReaderType SupportedReaders => ReaderType.AllReaders;

        public bool IsEnabledForAll => true;

        public object Data
        {
            get
            {
                var aPercentage = aCounts.Select(a => Math.Round((double)a / (double)sequenceCount * 100.0, 3));
                var cPercentage = cCounts.Select(c => Math.Round((double)c / (double)sequenceCount * 100.0, 3));
                var tPercentage = tCounts.Select(t => Math.Round((double)t / (double)sequenceCount * 100.0, 3));
                var gPercentage = gCounts.Select(g => Math.Round((double)g / (double)sequenceCount * 100.0, 3));

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

            var read = sequence.Read;
            for (var i = 0; i < sequenceLength; i++)
            {
                switch (read[i])
                {
                    case (byte)'A': aCounts[i]++; break;
                    case (byte)'C': cCounts[i]++; break;
                    case (byte)'T': tCounts[i]++; break;
                    case (byte)'G': gCounts[i]++; break;
                }
            }
        }

        public void Reset()
        {
            aCounts = Array.Empty<ulong>();
            cCounts = Array.Empty<ulong>();
            tCounts = Array.Empty<ulong>();
            gCounts = Array.Empty<ulong>();
            sequenceCount = 0;
        }
    }
}