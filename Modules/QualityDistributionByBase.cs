using System;
using System.Collections.Generic;
using System.Linq;

namespace Ovation.FasterQC.Net
{
    public class QualityDistributionByBase : IQcModule
    {
        private const byte ILLUMINA_BASE_ADJUSTMENT = 33;

        private readonly IDictionary<byte, ulong[]> qualityScores = new Dictionary<byte, ulong[]>();

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
                    aDistribution = qualityScores[(byte)'A'].Skip(lowestScore).Take(highestScore - lowestScore + 1),
                    cDistribution = qualityScores[(byte)'C'].Skip(lowestScore).Take(highestScore - lowestScore + 1),
                    tDistribution = qualityScores[(byte)'T'].Skip(lowestScore).Take(highestScore - lowestScore + 1),
                    gDistribution = qualityScores[(byte)'G'].Skip(lowestScore).Take(highestScore - lowestScore + 1),
                };
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var quals = sequence.Quality.ToArray();
            var chars = sequence.Read.ToArray();

            for (var i = 0; i < quals.Length; i++)
            {
                lowestScore = Math.Min(lowestScore, quals[i]);
                highestScore = Math.Max(highestScore, quals[i]);

                if (qualityScores.ContainsKey(chars[i]) == false)
                {
                    qualityScores.Add(chars[i], new ulong[128]);
                }

                qualityScores[chars[i]][quals[i]]++;
            }
        }

        public void Reset()
        {
            qualityScores.Clear();

            lowestScore = byte.MaxValue;
            highestScore = byte.MinValue;
        }
    }
}