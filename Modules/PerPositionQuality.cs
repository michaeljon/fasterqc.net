using System;
using System.Collections.Generic;

namespace Ovation.FasterQC.Net
{
    public class PerPositionQuality : IQcModule
    {
        private QualityMetric[] qualities = Array.Empty<QualityMetric>();

        private int minimumReadLength = int.MaxValue;
        private int maximumReadLength = int.MinValue;

        public string Name => "perPositionQuality";

        public string Description => "Calculates the per-position quality metrics";

        public object Data
        {
            get
            {
                var metrics = new object[maximumReadLength];

                for (var position = 0; position < qualities.Length; position++)
                {
                    metrics[position] = qualities[position].Metric;
                }

                return metrics;
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Quality.Length;

            minimumReadLength = Math.Min(minimumReadLength, sequenceLength);
            maximumReadLength = Math.Max(maximumReadLength, sequenceLength);

            // see if we need to resize this
            if (sequenceLength > qualities.Length)
            {
                Array.Resize(ref qualities, sequenceLength);
            }

            var qual = sequence.Quality;
            for (var q = 0; q < sequenceLength; q++)
            {
                if (qualities[q] == null)
                {
                    qualities[q] = new QualityMetric(q + 1);
                }

                qualities[q].lowestScore = Math.Min(qualities[q].lowestScore, qual[q]);
                qualities[q].highestScore = Math.Max(qualities[q].highestScore, qual[q]);
                qualities[q].qualityScores[qual[q]]++;
            }
        }

        public void Reset()
        {
            minimumReadLength = int.MaxValue;
            maximumReadLength = int.MinValue;

            qualities = Array.Empty<QualityMetric>();
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

                var sumOfDistribution = 0.0;
                foreach (var count in distribution.Values)
                {
                    sumOfDistribution += count;
                }

                var mean = (double)sumOfValues / (double)sumOfDistribution;

                var sumOfSquares = 0.0;
                foreach (var (pos, count) in distribution)
                {
                    sumOfSquares += Math.Pow((ulong)pos - mean, 2) * count;
                }

                var stdev = Math.Sqrt(sumOfSquares / sumOfDistribution);

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