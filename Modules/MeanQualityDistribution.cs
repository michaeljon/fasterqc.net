using System;
using System.Linq;

namespace Ovation.FasterQC.Net
{
    public class MeanQualityDistribution : IQcModule
    {
        private const byte ILLUMINA_BASE_ADJUSTMENT = 33;

        // the lowest quality score is '!' and the highest is '~',
        // but, we're just going to record an integer in every
        // position and fix it when someone asks
        private readonly ulong[] qualityScores = new ulong[128];

        private byte lowestScore = byte.MaxValue;

        private byte highestScore = byte.MinValue;

        public string Name => "meanQualityDistribution";

        public string Description => "Calculates the quality distribution across all sequences";

        public bool IsEnabledForAll => true;

        public object Data
        {
            get
            {
                return new
                {
                    lowestScore = lowestScore - ILLUMINA_BASE_ADJUSTMENT,
                    highestScore = highestScore - ILLUMINA_BASE_ADJUSTMENT,
                    distribution = qualityScores.Skip(lowestScore).Take(highestScore - lowestScore + 1)
                };
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Quality.Length;

            var sum = 0;
            var count = 0;

            var qual = sequence.Quality;
            for (var q = 0; q < sequenceLength; q++)
            {
                lowestScore = Math.Min(lowestScore, qual[q]);
                highestScore = Math.Max(highestScore, qual[q]);

                sum += qual[q];
                count++;
            }

            var mean = sum / count;
            qualityScores[mean]++;
        }

        public void Reset()
        {
            for (var p = 0; p < qualityScores.Length; p++)
            {
                qualityScores[p] = 0;
            }

            lowestScore = byte.MaxValue;
            highestScore = byte.MinValue;
        }
    }
}