using System;
using System.Collections.Generic;

namespace Ovation.FasterQC.Net
{
    public class PerPositionQuality : IQcModule
    {
        private readonly IDictionary<int, QualityMetric> qualities = new Dictionary<int, QualityMetric>();

        private int minimumReadLength = int.MaxValue;
        private int maximumReadLength = int.MinValue;

        public string Name => "perPositionQuality";

        public string Description => "Calculates the per-position quality metrics";

        public object Data
        {
            get
            {
                var metrics = new QualityMetric[maximumReadLength];

                foreach (var (position, quality) in qualities)
                {
                    metrics[position] = quality;
                }

                return metrics;
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Quality.Length;

            minimumReadLength = Math.Min(minimumReadLength, sequenceLength);
            maximumReadLength = Math.Max(maximumReadLength, sequenceLength);

            var chars = sequence.Quality.ToArray();
            for (var c = 0; c < chars.Length; c++)
            {
                if (qualities.ContainsKey(c) == false)
                {
                    qualities[c] = new QualityMetric(c + 1);
                }

                qualities[c].lowestScore = Math.Min(qualities[c].lowestScore, chars[c]);
                qualities[c].highestScore = Math.Max(qualities[c].highestScore, chars[c]);
                qualities[c].qualityScores[chars[c]]++;
            }
        }

        public void Reset()
        {
            minimumReadLength = int.MaxValue;
            maximumReadLength = int.MinValue;

            qualities.Clear();
        }
    }

    internal class QualityMetric
    {
        private const byte ILLUMINA_BASE_ADJUSTMENT = 33;

        public readonly ulong[] qualityScores = new ulong[128];

        public byte lowestScore = byte.MaxValue;

        public byte highestScore = byte.MinValue;

        public readonly int readPosition;

        public QualityMetric(int readPosition)
        {
            this.readPosition = readPosition;
        }

        public object Metric
        {
            get
            {
                var distribution = new Dictionary<int, ulong>();

                for (var score = lowestScore; score <= highestScore; score++)
                {
                    distribution.Add(score - ILLUMINA_BASE_ADJUSTMENT, qualityScores[score]);
                }

                var sumOfValues = 0UL;
                foreach (var (pos, count) in distribution)
                {
                    sumOfValues += (ulong)pos * count;
                }

                var sumOfDistribution = 0UL;
                foreach (var count in distribution.Values)
                {
                    sumOfDistribution += count;
                }

                var mean = sumOfValues / sumOfDistribution;

                var sumOfSquares = 0UL;
                foreach (var (pos, count) in distribution)
                {
                    sumOfValues += (ulong)Math.Pow((ulong)pos - mean, 2) * count;
                }

                var stdev = Math.Round(Math.Sqrt(sumOfSquares / sumOfDistribution), 3);

                return new
                {
                    position = readPosition,
                    lowestScore = lowestScore - ILLUMINA_BASE_ADJUSTMENT,
                    highestScore = highestScore - ILLUMINA_BASE_ADJUSTMENT,
                    mean,
                    stdev,
                    distribution
                };
            }
        }
    }
}