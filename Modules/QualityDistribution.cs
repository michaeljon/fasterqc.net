using System;
using System.Linq;

namespace Ovation.FasterQC.Net
{
    public class QualityDistribution : IQcModule
    {
        private const byte ILLUMINA_BASE_ADJUSTMENT = 33;

        private readonly ulong[] qualities = new ulong[128];

        private byte lowestScore = byte.MaxValue;

        private byte highestScore = byte.MinValue;

        public string Name => "qualityDistribution";

        public string Description => "Calculates the distribution of quality scores";

        public bool IsEnabledForAll => true;

        public object Data
        {
            get
            {
                return new
                {
                    lowestScore = lowestScore - ILLUMINA_BASE_ADJUSTMENT,
                    highestScore = highestScore - ILLUMINA_BASE_ADJUSTMENT,

                    lowestScoreUnadjusted = lowestScore,
                    highestScoreUnadjusted = highestScore,

                    qualities = qualities.Skip(lowestScore).Take(highestScore - lowestScore + 1),
                };
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;
            var quals = sequence.Quality;

            for (var i = 0; i < sequenceLength; i++)
            {
                var qual = quals[i];

                lowestScore = Math.Min(lowestScore, quals[i]);
                highestScore = Math.Max(highestScore, quals[i]);

                qualities[qual]++;
            }
        }

        public void Reset()
        {
            for (var p = 0; p < qualities.Length; p++)
            {
                qualities[p] = 0;
            }

            lowestScore = byte.MaxValue;
            highestScore = byte.MinValue;
        }
    }
}
