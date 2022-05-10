using System;

namespace Ovation.FasterQC.Net
{
    public class BasicStatistics : IQcModule
    {
        private long sequenceCount;

        private int minimumReadLength = int.MaxValue;
        private int maximumReadLength = int.MinValue;

        private long totalBases;

        private long aCount;
        private long tCount;
        private long cCount;
        private long gCount;
        private long nCount;
        private long xCount;

        private byte minimumQuality = byte.MaxValue;

        public string Name => "basicStats";

        public string Description => "Calculates basic quality statistics";

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;

            totalBases += sequenceLength;
            sequenceCount++;

            minimumReadLength = Math.Min(minimumReadLength, sequenceLength);
            maximumReadLength = Math.Max(maximumReadLength, sequenceLength);

            var chars = sequence.Read.ToArray();
            for (var c = 0; c < chars.Length; c++)
            {
                switch (chars[c])
                {
                    case (byte)'G': gCount++; break;
                    case (byte)'A': aCount++; break;
                    case (byte)'T': tCount++; break;
                    case (byte)'C': cCount++; break;
                    case (byte)'N': nCount++; break;
                    default:
                        xCount++;
                        break;
                }
            }

            var qual = sequence.Quality.ToArray();
            for (var c = 0; c < qual.Length; c++)
            {
                minimumQuality = Math.Min(minimumQuality, qual[c]);
            }
        }

        public void Reset()
        {
            sequenceCount = 0;

            minimumReadLength = int.MaxValue;
            maximumReadLength = int.MinValue;

            aCount = 0;
            tCount = 0;
            cCount = 0;
            gCount = 0;
            nCount = 0;
            xCount = 0;

            minimumQuality = byte.MaxValue;
        }

        public object Data => new
        {
            sequenceCount,

            totalBases,

            aCount,
            tCount,
            cCount,
            gCount,
            nCount,
            xCount,

            minimumQuality,

            gcContent = (double)(cCount + gCount) / (double)totalBases,

            minimumReadLength,
            maximumReadLength
        };
    }
}