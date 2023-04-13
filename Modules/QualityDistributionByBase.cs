// using System;
// using System.Linq;
//
// namespace Ovation.FasterQC.Net
// {
//     public class QualityDistributionByBase : IQcModule
//     {
//         private const byte ILLUMINA_BASE_ADJUSTMENT = 33;
//
//         private readonly ulong[] aQuality = new ulong[128];
//
//         private readonly ulong[] cQuality = new ulong[128];
//
//         private readonly ulong[] tQuality = new ulong[128];
//
//         private readonly ulong[] gQuality = new ulong[128];
//
//         private byte lowestScore = byte.MaxValue;
//
//         private byte highestScore = byte.MinValue;
//
//         public string Name => "qualityDistributionByBase";
//
//         public string Description => "Calculates the quality distribution across all sequences";
//
//         public bool IsEnabledForAll => true;
//
//         public ReaderType SupportedReaders => ReaderType.AllReaders;
//
//         public object Data
//         {
//             get
//             {
//                 return new
//                 {
//                     lowestScore = lowestScore - ILLUMINA_BASE_ADJUSTMENT,
//                     highestScore = highestScore - ILLUMINA_BASE_ADJUSTMENT,
//
//                     lowestScoreUnadjusted = lowestScore,
//                     highestScoreUnadjusted = highestScore,
//
//                     aDistribution = aQuality.Skip(lowestScore).Take(highestScore - lowestScore + 1),
//                     cDistribution = cQuality.Skip(lowestScore).Take(highestScore - lowestScore + 1),
//                     tDistribution = tQuality.Skip(lowestScore).Take(highestScore - lowestScore + 1),
//                     gDistribution = gQuality.Skip(lowestScore).Take(highestScore - lowestScore + 1),
//                 };
//             }
//         }
//
//         public void ProcessSequence(Sequence sequence)
//         {
//             var sequenceLength = sequence.Read.Length;
//
//             var quals = sequence.Quality;
//             var chars = sequence.Read;
//
//             for (var i = 0; i < sequenceLength; i++)
//             {
//                 var qual = quals[i];
//                 var read = chars[i];
//
//                 if (read != (byte)'N')
//                 {
//                     lowestScore = Math.Min(lowestScore, quals[i]);
//                     highestScore = Math.Max(highestScore, quals[i]);
//                 }
//
//                 switch (read)
//                 {
//                     case (byte)'A': aQuality[qual]++; break;
//                     case (byte)'C': cQuality[qual]++; break;
//                     case (byte)'T': tQuality[qual]++; break;
//                     case (byte)'G': gQuality[qual]++; break;
//                 }
//             }
//         }
//
//         public void Reset()
//         {
//             for (var p = 0; p < aQuality.Length; p++)
//             {
//                 aQuality[p] = 0;
//                 cQuality[p] = 0;
//                 tQuality[p] = 0;
//                 gQuality[p] = 0;
//             }
//
//             lowestScore = byte.MaxValue;
//             highestScore = byte.MinValue;
//         }
//     }
// }
