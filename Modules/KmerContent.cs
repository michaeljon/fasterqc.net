using System;
using System.Collections.Generic;

namespace Ovation.FasterQC.Net
{
    public class KMerContent : IQcModule
    {
        private static readonly int KMER_SIZE = 4;

        private static readonly int DICTIONARY_SIZE = (int)Math.Pow(4, KMER_SIZE);

        private readonly ulong[] kmers = new ulong[DICTIONARY_SIZE];

        public string Name => "kmerContent";

        public string Description => "Computes 4-mer counts across all sequences";

        public object Data
        {
            get
            {
                var result = new Dictionary<string, ulong>(DICTIONARY_SIZE);
                var index = new char[4];

                for (int k = 0; k < DICTIONARY_SIZE; k++)
                {
                    var count = kmers[k];
                    if (count == 0)
                        continue;

                    index[3] = ((k & 0b11000000) >> 6) switch
                    {
                        0b00 => 'A',
                        0b01 => 'C',
                        0b10 => 'T',
                        0b11 => 'G',
                        _ => ' '
                    };

                    index[2] = ((k & 0b00110000) >> 4) switch
                    {
                        0b00 => 'A',
                        0b01 => 'C',
                        0b10 => 'T',
                        0b11 => 'G',
                        _ => ' '
                    };

                    index[1] = ((k & 0b00001100) >> 2) switch
                    {
                        0b00 => 'A',
                        0b01 => 'C',
                        0b10 => 'T',
                        0b11 => 'G',
                        _ => ' '
                    };

                    index[0] = (k & 0b00000011) switch
                    {
                        0b00 => 'A',
                        0b01 => 'C',
                        0b10 => 'T',
                        0b11 => 'G',
                        _ => ' '
                    };

                    var kmer = new string(index);
                    Console.Error.WriteLine($"{k} => {kmer} => {kmers[k]}");

                    result.Add(kmer, count);
                }

                return result;
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;
            var read = sequence.Read;

            if (sequenceLength < KMER_SIZE)
            {
                return;
            }

            for (var s = 0; s < sequenceLength - KMER_SIZE; s++)
            {
                byte index = 0;

                // we're not going to count any kmers with an N, we could be smarter and set s to the
                // last read with an N to shorten our traversal, but for now...
                if (read[s] == (byte)'N' || read[s + 1] == (byte)'N' || read[s + 2] == (byte)'N' || read[s + 3] == (byte)'N')
                {
                    continue;
                }

                // loop unroll
                index |= (byte)(read[s] switch
                {
                    (byte)'A' => 0b00,
                    (byte)'C' => 0b01,
                    (byte)'T' => 0b10,
                    (byte)'G' => 0b11,
                    _ => 0
                } << 6);

                index |= (byte)(read[s + 1] switch
                {
                    (byte)'A' => 0b00,
                    (byte)'C' => 0b01,
                    (byte)'T' => 0b10,
                    (byte)'G' => 0b11,
                    _ => 0
                } << 4);

                index |= (byte)(read[s + 2] switch
                {
                    (byte)'A' => 0b00,
                    (byte)'C' => 0b01,
                    (byte)'T' => 0b10,
                    (byte)'G' => 0b11,
                    _ => 0
                } << 2);

                index |= (byte)(read[s + 3] switch
                {
                    (byte)'A' => 0b00,
                    (byte)'C' => 0b01,
                    (byte)'T' => 0b10,
                    (byte)'G' => 0b11,
                    _ => 0
                });

                kmers[index]++;
            }
        }

        public void Reset()
        {
            for (var k = 0; k < DICTIONARY_SIZE; k++)
            {
                kmers[k] = 0;
            }
        }
    }
}