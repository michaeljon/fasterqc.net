using System;
using System.Linq;

namespace Ovation.FasterQC.Net
{
    public class QualityDistributionByBase : IQcModule
    {
        private ulong[] aQuality = Array.Empty<ulong>();
        private ulong[] cQuality = Array.Empty<ulong>();
        private ulong[] tQuality = Array.Empty<ulong>();
        private ulong[] gQuality = Array.Empty<ulong>();


        private byte lowestScore = byte.MaxValue;

        private byte highestScore = byte.MinValue;

        public string Name => "qualityDistributionByBase";

        public string Description => "Calculates the quality distribution across all sequences";

        public object Data
        {
            get
            {
                return new
                {
                    aDistribution = aQuality.Skip(lowestScore).Take(highestScore - lowestScore + 1),
                    cDistribution = cQuality.Skip(lowestScore).Take(highestScore - lowestScore + 1),
                    tDistribution = tQuality.Skip(lowestScore).Take(highestScore - lowestScore + 1),
                    gDistribution = gQuality.Skip(lowestScore).Take(highestScore - lowestScore + 1),
                };
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;

            var quals = sequence.Quality;
            var chars = sequence.Read;

            // see if we need to resize this
            if (sequenceLength > aQuality.Length)
            {
                Array.Resize(ref aQuality, sequenceLength);
                Array.Resize(ref cQuality, sequenceLength);
                Array.Resize(ref tQuality, sequenceLength);
                Array.Resize(ref gQuality, sequenceLength);
            }

            for (var i = 0; i < sequenceLength; i++)
            {
                var qual = quals[i];
                var read = chars[i];

                lowestScore = Math.Min(lowestScore, quals[i]);
                highestScore = Math.Max(highestScore, quals[i]);

                switch (read)
                {
                    case (byte)'A': aQuality[qual]++; break;
                    case (byte)'C': cQuality[qual]++; break;
                    case (byte)'T': tQuality[qual]++; break;
                    case (byte)'G': gQuality[qual]++; break;
                }
            }
        }

        public void Reset()
        {
            aQuality = Array.Empty<ulong>();
            cQuality = Array.Empty<ulong>();
            tQuality = Array.Empty<ulong>();
            gQuality = Array.Empty<ulong>();

            lowestScore = byte.MaxValue;
            highestScore = byte.MinValue;
        }
    }
}