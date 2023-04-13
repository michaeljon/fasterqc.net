using System;
using System.Collections.Generic;
using System.Linq;

namespace Ovation.FasterQC.Net
{
    public record BasicStatisticsResult(ulong SequenceCount, ulong TotalBases, ulong ACount, ulong TCount, ulong CCount,
        ulong GCount, ulong NCount, ulong XCount, byte MinimumQuality, double GCContent, int MinimumReadLength,
        int MaximumReadLength);

    public class BasicStatistics : IQcModule<BasicStatisticsResult, BasicStatisticsResult>
    {

        private const byte ILLUMINA_BASE_ADJUSTMENT = 33;

        public string Name => "basicStats";

        public string Description => "Calculates basic quality statistics";

        public bool IsEnabledForAll => true;

        public ReaderType SupportedReaders => ReaderType.AllReaders;

        public BasicStatisticsResult Map(IEnumerable<Sequence> sequences)
        {
            ulong sequenceCount = 0;

            int minimumReadLength = int.MaxValue;
            int maximumReadLength = int.MinValue;

            ulong totalBases = 0;

            ulong aCount = 0;
            ulong tCount = 0;
            ulong cCount = 0;
            ulong gCount = 0;
            ulong nCount = 0;
            ulong xCount = 0;

            byte minimumQuality = byte.MaxValue;

            foreach (var sequence in sequences)
            {
                // var sequenceLength = sequence.Read.Length;
                //
                // totalBases += (ulong)sequenceLength;
                sequenceCount++;

                // minimumReadLength = Math.Min(minimumReadLength, sequenceLength);
                // maximumReadLength = Math.Max(maximumReadLength, sequenceLength);
                //
                // var reads = sequence.Read.ToCharArray();
                // var quals = sequence.Quality.ToCharArray();
                //
                // for (var i = 0; i < sequenceLength; i++)
                // {
                //     switch (reads[i])
                //     {
                //         case 'G': gCount++; break;
                //         case 'A': aCount++; break;
                //         case 'T': tCount++; break;
                //         case 'C': cCount++; break;
                //         case 'N': nCount++; break;
                //         default:
                //             xCount++;
                //             break;
                //     }
                //
                //     minimumQuality = Math.Min(minimumQuality, (byte)quals[i]);
                // }
            }

            return new BasicStatisticsResult(
                SequenceCount: sequenceCount,
                TotalBases: totalBases,
                ACount: aCount,
                TCount: tCount,
                CCount: cCount,
                GCount: gCount,
                NCount: nCount,
                XCount: xCount,
                MinimumQuality: minimumQuality,
                GCContent: 0.0,
                MinimumReadLength: minimumReadLength,
                MaximumReadLength: maximumReadLength);
        }

        public BasicStatisticsResult Reduce(IEnumerable<BasicStatisticsResult> mapResults)
        {
            var combined = mapResults.Aggregate(new BasicStatisticsResult(
                    SequenceCount: 0,
                    TotalBases: 0,
                    ACount: 0,
                    TCount: 0,
                    CCount: 0,
                    GCount: 0,
                    NCount: 0,
                    XCount: 0,
                    MinimumQuality: byte.MaxValue,
                    GCContent: 0.0,
                    MinimumReadLength: int.MaxValue,
                    MaximumReadLength: int.MinValue),
                (acc, cur) => new BasicStatisticsResult(
                    SequenceCount: acc.SequenceCount + cur.SequenceCount,
                    TotalBases: acc.TotalBases + cur.TotalBases,
                    ACount: acc.ACount + cur.ACount,
                    TCount: acc.TCount + cur.TCount,
                    CCount: acc.CCount + cur.CCount,
                    GCount: acc.GCount + cur.GCount,
                    NCount: acc.NCount + cur.NCount,
                    XCount: acc.XCount + cur.XCount,
                    MinimumQuality: Math.Min(acc.MinimumQuality, cur.MinimumQuality),
                    GCContent: 0.0,
                    MinimumReadLength: Math.Min(acc.MinimumReadLength, cur.MinimumReadLength),
                    MaximumReadLength: Math.Max(acc.MaximumReadLength, cur.MaximumReadLength)));

            return combined with { MinimumQuality = (byte)Math.Max(combined.MinimumQuality - ILLUMINA_BASE_ADJUSTMENT, 0), GCContent = combined.TotalBases == 0 ? 0.0 : Math.Round((double)(combined.CCount + combined.GCount) / (double)combined.TotalBases * 100.0, 3) };
        }
    }
}
