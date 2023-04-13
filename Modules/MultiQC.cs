// using System;
// using System.Linq;
//
// namespace Ovation.FasterQC.Net
// {
//     public class MultiQC : IQcModule
//     {
//         private const byte ILLUMINA_BASE_ADJUSTMENT_LOWER = 33;
//
//         private const byte ILLUMINA_BASE_ADJUSTMENT_UPPER = 126;
//
//         private int maxReadLength = 0;
//
//         private ulong sequenceCount = 0;
//
//         private ulong[][] qualityScoresAtPosition = Array.Empty<ulong[]>();
//
//         private uint lowestMean = uint.MaxValue;
//
//         private uint highestMean = uint.MinValue;
//
//         private ulong[] nCountsAtPosition = Array.Empty<ulong>();
//
//         private readonly ulong[] gcPercentageCounts = new ulong[101];
//
//         private readonly ulong[] meanPhredCounts = new ulong[128];
//
//         public string Name => "multiqc";
//
//         public string Description => "Generates limited / Ovation-branded output compatible with MultiQC";
//
//         public bool IsEnabledForAll => true;
//
//         public ReaderType SupportedReaders => ReaderType.AllReaders;
//
//         public object Data
//         {
//             get
//             {
//                 var meanPhreds = new ulong[maxReadLength][];
//                 for (var pos = 0; pos < maxReadLength; pos++)
//                 {
//                     meanPhreds[pos] = new ulong[128];
//                     for (var qual = 0UL; qual < 128; qual++)
//                     {
//                         meanPhreds[pos][qual] = qualityScoresAtPosition[pos][qual] * qual;
//                     }
//                 }
//
//                 var phreds = meanPhredCounts
//                                 .Skip(ILLUMINA_BASE_ADJUSTMENT_LOWER)
//                                 .Take(ILLUMINA_BASE_ADJUSTMENT_UPPER - ILLUMINA_BASE_ADJUSTMENT_LOWER + 1)
//                                 .ToArray();
//
//                 return new
//                 {
//                     meanPhredByPosition = meanPhreds
//                         .Select((p, idx) => new { idx, val = (int)(p.Sum(q => (decimal)q) / sequenceCount) })
//                         .ToDictionary(i => i.idx + 1, i => i.val),
//
//                     gcPercentageCounts = gcPercentageCounts
//                         .Select((gcc, idx) => new { idx, gcc })
//                         .ToDictionary(i => i.idx, i => i.gcc),
//
//                     gcPercentagePercentage = gcPercentageCounts
//                         .Select((gcc, idx) => new { idx, val = Math.Round((float)gcc / (float)sequenceCount * 100.0, 3) })
//                         .ToDictionary(i => i.idx, i => i.val),
//
//                     nCountsAtPosition = nCountsAtPosition
//                         .Select((nc, idx) => new { idx, nc })
//                         .ToDictionary(i => i.idx + 1, i => i.nc),
//
//                     nPercentageAtPosition = nCountsAtPosition
//                         .Select((nc, idx) => new { idx, val = Math.Round((float)nc / (float)sequenceCount * 100.0, 3) })
//                         .ToDictionary(i => i.idx + 1, i => i.val),
//
//                     meanPhredCounts = meanPhredCounts
//                         .Skip(ILLUMINA_BASE_ADJUSTMENT_LOWER)
//                         .Take(ILLUMINA_BASE_ADJUSTMENT_UPPER - ILLUMINA_BASE_ADJUSTMENT_LOWER + 1)
//                         .Select((val, idx) => new { idx, val })
//                         .ToDictionary(i => ILLUMINA_BASE_ADJUSTMENT_LOWER + i.idx, i => i.val),
//
//                     meanPhredPercentage = meanPhredCounts
//                         .Skip(ILLUMINA_BASE_ADJUSTMENT_LOWER)
//                         .Take(ILLUMINA_BASE_ADJUSTMENT_UPPER - ILLUMINA_BASE_ADJUSTMENT_LOWER + 1)
//                         .Select((pc, idx) => new { idx, val = Math.Round((float)pc / (float)sequenceCount * 100.0, 3) })
//                         .ToDictionary(i => ILLUMINA_BASE_ADJUSTMENT_LOWER + i.idx, i => i.val),
//
//                     phreds = phreds
//                         .Select((val, idx) => new { phred = Convert.ToChar(ILLUMINA_BASE_ADJUSTMENT_LOWER + idx), count = val })
//                         .ToDictionary(i => i.phred, i => i.count)
//                 };
//             }
//         }
//
//         public void ProcessSequence(Sequence sequence)
//         {
//             sequenceCount++;
//
//             var sequenceLength = sequence.Read.Length;
//
//             // see if we need to resize our arrays
//             if (sequenceLength > maxReadLength)
//             {
//                 maxReadLength = sequenceLength;
//
//                 Array.Resize(ref qualityScoresAtPosition, maxReadLength);
//                 for (var n = 0; n < maxReadLength; n++)
//                 {
//                     if (qualityScoresAtPosition[n] == null)
//                     {
//                         qualityScoresAtPosition[n] = new ulong[128];
//                     }
//                 }
//                 Array.Resize(ref nCountsAtPosition, maxReadLength);
//             }
//
//             // calculate mean quality score
//             var sum = 0;
//             var count = 0;
//
//             var qual = sequence.Quality;
//             for (var i = 0; i < sequenceLength; i++)
//             {
//                 byte q = qual[i];
//
//                 qualityScoresAtPosition[i][q]++;
//                 sum += q;
//                 count++;
//             }
//
//             var mean = (uint)((float)sum / (float)count);
//             meanPhredCounts[mean]++;
//
//             lowestMean = Math.Min(lowestMean, mean);
//             highestMean = Math.Max(highestMean, mean);
//
//             // calculate the gc percentage and ncount of the read
//             var gcCount = 0;
//             var read = sequence.Read;
//             for (var i = 0; i < sequenceLength; i++)
//             {
//                 switch (read[i])
//                 {
//                     case (byte)'G':
//                     case (byte)'C':
//                         gcCount++;
//                         break;
//
//                     case (byte)'N':
//                         nCountsAtPosition[i]++;
//                         break;
//
//                     default:
//                         break;
//                 }
//             }
//
//             var gcPercentage = (int)((float)gcCount / (float)sequenceLength * 100.0);
//             gcPercentageCounts[gcPercentage]++;
//         }
//
//         public void Reset()
//         {
//         }
//     }
// }
