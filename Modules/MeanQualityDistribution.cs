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

        private uint lowestMean = uint.MaxValue;

        private uint highestMean = uint.MinValue;

        public string Name => "meanQualityDistribution";

        public string Description => "Calculates the quality distribution across all sequences";

        public bool IsEnabledForAll => true;

        public object Data
        {
            get
            {
                return new
                {
                    lowestMean = lowestMean - ILLUMINA_BASE_ADJUSTMENT,
                    highestMean = highestMean - ILLUMINA_BASE_ADJUSTMENT,
                    distribution = qualityScores.Skip((int)lowestMean).Take((int)(highestMean - lowestMean + 1))
                };
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Quality.Length;

            var sum = 0;
            var count = 0;

            var qual = sequence.Quality;
            for (var i = 0; i < sequenceLength; i++)
            {
                sum += qual[i];
                count++;
            }

            var mean = (uint)(sum / count);

            lowestMean = Math.Min(lowestMean, mean);
            highestMean = Math.Max(highestMean, mean);

            qualityScores[mean]++;
        }

        public void Reset()
        {
            for (var p = 0; p < qualityScores.Length; p++)
            {
                qualityScores[p] = 0;
            }

            lowestMean = uint.MaxValue;
            highestMean = uint.MinValue;
        }
    }
}