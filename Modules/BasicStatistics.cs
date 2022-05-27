using System;

namespace Ovation.FasterQC.Net
{
    public class BasicStatistics : IQcModule
    {
        private const byte ILLUMINA_BASE_ADJUSTMENT = 33;

        private ulong sequenceCount;

        private int minimumReadLength = int.MaxValue;
        private int maximumReadLength = int.MinValue;

        private ulong totalBases;

        private ulong aCount;
        private ulong tCount;
        private ulong cCount;
        private ulong gCount;
        private ulong nCount;
        private ulong xCount;

        private byte minimumQuality = byte.MaxValue;

        public string Name => "basicStats";

        public string Description => "Calculates basic quality statistics";

        public bool IsEnabledForAll => true;

        public ReaderType SupportedReaders => ReaderType.AllReaders;

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;

            totalBases += (ulong)sequenceLength;
            sequenceCount++;

            minimumReadLength = Math.Min(minimumReadLength, sequenceLength);
            maximumReadLength = Math.Max(maximumReadLength, sequenceLength);

            var reads = sequence.Read;
            var quals = sequence.Quality;

            for (var i = 0; i < sequenceLength; i++)
            {
                switch (reads[i])
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

                minimumQuality = Math.Min(minimumQuality, quals[i]);
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

            minimumQuality = Math.Min(minimumQuality - ILLUMINA_BASE_ADJUSTMENT, 0),

            gcContent = Math.Round((double)(cCount + gCount) / (double)totalBases * 100.0, 3),

            minimumReadLength,
            maximumReadLength
        };
    }
}