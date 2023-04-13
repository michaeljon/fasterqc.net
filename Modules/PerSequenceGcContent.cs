// using System;
// using System.Linq;
//
// namespace Ovation.FasterQC.Net
// {
//     public class PerSequenceGcContent : IQcModule
//     {
//         private ulong[] gcCounts = Array.Empty<ulong>();
//
//         private ulong sequenceCount;
//
//         public string Name => "gcDistribution";
//
//         public string Description => "Distribution of GC content percentages";
//
//         public bool IsEnabledForAll => true;
//
//         public ReaderType SupportedReaders => ReaderType.AllReaders;
//
//         public object Data => gcCounts.Select(a => Math.Round((double)a / (double)sequenceCount * 100.0, 3));
//
//         public void ProcessSequence(Sequence sequence)
//         {
//             var sequenceLength = sequence.Read.Length;
//             sequenceCount++;
//
//             // see if we need to resize this
//             if (sequenceLength > gcCounts.Length)
//             {
//                 Array.Resize(ref gcCounts, sequenceLength);
//             }
//
//             var read = sequence.Read;
//             for (var i = 0; i < sequenceLength; i++)
//             {
//                 if (read[i] == 'G' || read[i] == 'C')
//                 {
//                     gcCounts[i]++;
//                 }
//             }
//         }
//
//         public void Reset()
//         {
//             gcCounts = Array.Empty<ulong>();
//         }
//     }
// }
