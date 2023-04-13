// using System;
// using System.Collections.Generic;
//
// namespace Ovation.FasterQC.Net
// {
//     public class KMerContent : IQcModule
//     {
//         //
//         // pay careful attention to the structure of this code, it's not a
//         // typical set of loops. at the expensive of some binary size
//         // we're trading off loop management
//         //
//         // https://en.wikipedia.org/wiki/Loop_unrolling
//         //
//         private static readonly int KMER_SIZE = 4;
//
//         private static readonly int DICTIONARY_SIZE = (int)Math.Pow(4, KMER_SIZE);
//
//         private readonly ulong[] kmers = new ulong[DICTIONARY_SIZE];
//
//         public string Name => "kmerContent";
//
//         public string Description => "Computes 4-mer counts across all sequences";
//
//         public bool IsEnabledForAll => true;
//
//         public ReaderType SupportedReaders => ReaderType.AllReaders;
//
//         public object Data
//         {
//             get
//             {
//                 var result = new Dictionary<string, ulong>(DICTIONARY_SIZE);
//                 var kmer = new char[4];
//
//                 for (int i = 0; i < DICTIONARY_SIZE; i++)
//                 {
//                     var count = kmers[i];
//                     if (count == 0)
//                         continue;
//
//                     kmer[3] = ((i & 0b11000000) >> 6) switch
//                     {
//                         0b00 => 'A',
//                         0b01 => 'C',
//                         0b10 => 'T',
//                         0b11 => 'G',
//                         _ => ' '
//                     };
//
//                     kmer[2] = ((i & 0b00110000) >> 4) switch
//                     {
//                         0b00 => 'A',
//                         0b01 => 'C',
//                         0b10 => 'T',
//                         0b11 => 'G',
//                         _ => ' '
//                     };
//
//                     kmer[1] = ((i & 0b00001100) >> 2) switch
//                     {
//                         0b00 => 'A',
//                         0b01 => 'C',
//                         0b10 => 'T',
//                         0b11 => 'G',
//                         _ => ' '
//                     };
//
//                     kmer[0] = (i & 0b00000011) switch
//                     {
//                         0b00 => 'A',
//                         0b01 => 'C',
//                         0b10 => 'T',
//                         0b11 => 'G',
//                         _ => ' '
//                     };
//
//                     result.Add(new string(kmer), count);
//                 }
//
//                 return result;
//             }
//         }
//
//         public void ProcessSequence(Sequence sequence)
//         {
//             var sequenceLength = sequence.Read.Length;
//             var read = sequence.Read;
//
//             if (sequenceLength < KMER_SIZE)
//             {
//                 return;
//             }
//
//             for (var s = 0; s < sequenceLength - KMER_SIZE; s++)
//             {
//                 byte kmer = 0;
//
//                 // loop unroll and shift right if we find an N
//                 if (read[s + 3] == (byte)'N')
//                 {
//                     s += 3;
//                     continue;
//                 }
//
//                 if (read[s + 2] == (byte)'N')
//                 {
//                     s += 2;
//                     continue;
//                 }
//
//                 if (read[s + 1] == (byte)'N')
//                 {
//                     s += 1;
//                     continue;
//                 }
//
//                 if (read[s] == (byte)'N')
//                 {
//                     continue;
//                 }
//
//                 // loop unroll
//                 kmer |= (byte)(read[s] switch
//                 {
//                     (byte)'A' => 0b00,
//                     (byte)'C' => 0b01,
//                     (byte)'T' => 0b10,
//                     (byte)'G' => 0b11,
//                     _ => 0
//                 } << 6);
//
//                 kmer |= (byte)(read[s + 1] switch
//                 {
//                     (byte)'A' => 0b00,
//                     (byte)'C' => 0b01,
//                     (byte)'T' => 0b10,
//                     (byte)'G' => 0b11,
//                     _ => 0
//                 } << 4);
//
//                 kmer |= (byte)(read[s + 2] switch
//                 {
//                     (byte)'A' => 0b00,
//                     (byte)'C' => 0b01,
//                     (byte)'T' => 0b10,
//                     (byte)'G' => 0b11,
//                     _ => 0
//                 } << 2);
//
//                 kmer |= (byte)(read[s + 3] switch
//                 {
//                     (byte)'A' => 0b00,
//                     (byte)'C' => 0b01,
//                     (byte)'T' => 0b10,
//                     (byte)'G' => 0b11,
//                     _ => 0
//                 });
//
//                 kmers[kmer]++;
//             }
//         }
//
//         public void Reset()
//         {
//             for (var k = 0; k < DICTIONARY_SIZE; k++)
//             {
//                 kmers[k] = 0;
//             }
//         }
//     }
// }
