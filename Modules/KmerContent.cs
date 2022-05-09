using System.Collections.Generic;

namespace Ovation.FasterQC.Net
{
    public class KMerContent : IQcModule
    {
        private const int KMER_SIZE = 4;

        private static readonly IDictionary<uint, long> kmers = new Dictionary<uint, long>();

        public string Name => "kMer content";

        public string Description => "Computes kMer counts";

        public object Data
        {
            get
            {
                var result = new Dictionary<string, long>();

                foreach (var key in kmers.Keys)
                {
                    var count = kmers[key];
                    var index = new char[4];

                    index[0] = (char)(key & 0x000000ff);
                    index[1] = (char)((key & 0x0000ff00) >> 8);
                    index[2] = (char)((key & 0x00ff0000) >> 16);
                    index[3] = (char)((key & 0xff000000) >> 24);

                    if (index[0] == 'N' || index[1] == 'N' || index[2] == 'N' || index[3] == 'N')
                        continue;

                    result.Add(new string(index), count);
                }

                return result;
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;
            var chars = sequence.Read.ToArray();

            var index = (uint)(chars[0] << 24 |
                               chars[1] << 16 |
                               chars[2] << 8 |
                               chars[3]);

            for (var s = 1; s < sequenceLength - KMER_SIZE; s++)
            {
                if (kmers.ContainsKey(index) == false)
                {
                    kmers.Add(index, 0);
                }

                kmers[index]++;

                index <<= 8;
                index |= chars[s];
            }
        }

        public void Reset()
        {
            kmers.Clear();
        }
    }
}