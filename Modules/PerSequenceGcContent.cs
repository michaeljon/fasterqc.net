namespace Ovation.FasterQC.Net
{
    public class PerSequenceGcContent : IQcModule
    {
        private static readonly ulong[] gcContent = new ulong[101];

        public string Name => "gcDistribution";

        public string Description => "Distribution of GC content percentages";

        public object Data => gcContent;

        public void ProcessSequence(Sequence sequence)
        {
            var sequenceLength = sequence.Read.Length;

            var gcCount = 0;
            var chars = sequence.Read.ToArray();
            for (var c = 0; c < chars.Length; c++)
            {
                if (chars[c] == 'G' || chars[c] == 'C')
                {
                    gcCount++;
                }
            }

            var gcPercentage = (int)((double)gcCount / (double)sequenceLength * 100.0);

            gcContent[gcPercentage]++;
        }

        public void Reset()
        {
            for (var p = 0; p < gcContent.Length; p++)
            {
                gcContent[p] = 0;
            }
        }
    }
}