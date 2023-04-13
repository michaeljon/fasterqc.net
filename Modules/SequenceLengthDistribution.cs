// using System;
// using System.Collections.Generic;
//
// namespace Ovation.FasterQC.Net
// {
//     public class SequenceLengthDistribution : IQcModule
//     {
//         private int minimumReadLength = int.MaxValue;
//
//         private int maximumReadLength = int.MinValue;
//
//         private readonly IDictionary<int, ulong> lengths = new Dictionary<int, ulong>();
//
//         public string Name => "sequenceLengthDistribution";
//
//         public string Description => "Calculates the sequence length distributions";
//
//         public bool IsEnabledForAll => true;
//
//         public ReaderType SupportedReaders => ReaderType.AllReaders;
//
//         public object Data
//         {
//             get
//             {
//                 var distribution = new ulong[maximumReadLength + 1];
//
//                 foreach (var (length, count) in lengths)
//                 {
//                     distribution[length] = count;
//                 }
//
//                 return new
//                 {
//                     minimumReadLength,
//                     maximumReadLength,
//                     distribution
//                 };
//             }
//         }
//
//         public void ProcessSequence(Sequence sequence)
//         {
//             var sequenceLength = sequence.Read.Length;
//
//             minimumReadLength = Math.Min(minimumReadLength, sequenceLength);
//             maximumReadLength = Math.Max(maximumReadLength, sequenceLength);
//
//             if (lengths.ContainsKey(sequenceLength) == false)
//             {
//                 lengths.Add(sequenceLength, 0);
//             }
//
//             lengths[sequenceLength]++;
//         }
//
//         public void Reset()
//         {
//             minimumReadLength = int.MaxValue;
//             maximumReadLength = int.MinValue;
//
//             lengths.Clear();
//         }
//     }
// }
