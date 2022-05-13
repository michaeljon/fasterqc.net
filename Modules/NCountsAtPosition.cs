using System;

namespace Ovation.FasterQC.Net
{
    public class NCountsAtPosition : IQcModule
    {
        private int[] nCounts = Array.Empty<int>();
        private int[] notNCounts = Array.Empty<int>();

        public string Name => "nPercentages";

        public string Description => "Calculates N counts at position along sequence";

        public object Data
        {
            get
            {
                var result = new double[nCounts.Length];

                for (var position = 0; position < nCounts.Length; position++)
                {
                    var totalReads = nCounts[position] + notNCounts[position];
                    result[position] = Math.Round((double)nCounts[position] / (double)totalReads * 100.0, 3);
                }

                return new
                {
                    length = result.Length,
                    percentages = result
                };
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;

            // see if we need to resize this
            if (sequenceLength > nCounts.Length)
            {
                Array.Resize(ref nCounts, sequenceLength);
                Array.Resize(ref notNCounts, sequenceLength);
            }

            var read = sequence.Read;
            for (var i = 0; i < sequenceLength; i++)
            {
                if (read[i] == 'N')
                {
                    nCounts[i]++;
                }
                else
                {
                    notNCounts[i]++;
                }
            }
        }

        public void Reset()
        {
            nCounts = Array.Empty<int>();
            notNCounts = Array.Empty<int>();
        }
    }
}