
using System.Collections.Generic;
using System.Linq;

namespace Ovation.FasterQC.Net
{
    public class AlignmentStatistics : IQcModule
    {
        private ulong sequenceCount;

        private ulong baseCount;

        private ulong paired;

        private ulong aligned;

        private ulong alignedAndPaired;

        private ulong segmentUnmapped;

        private ulong nextSegmentUnmapped;

        private ulong reverseComplemented;

        private ulong nextSegmentReverseComplemented;

        private ulong nonPrimaryAlignment;

        private ulong failedQualityChecks;

        private ulong opticalDuplicate;

        private ulong alignedBases;

        private readonly Dictionary<int, ReadPair> readLengthHistogram = new();

        public string Name => "alignmentStatistics";

        public bool IsEnabledForAll => true;

        public string Description => "Calculates alignment statistics for SAM/BAM files";

        public void ProcessSequence(Sequence sequence)
        {
            sequenceCount++;
            baseCount += (ulong)sequence.Read.Length;

            if (readLengthHistogram.ContainsKey(sequence.Read.Length) == false)
            {
                readLengthHistogram.Add(sequence.Read.Length, new ReadPair());
            }

            var readPair = readLengthHistogram[sequence.Read.Length];

            if ((sequence.ReadFlag & ReadFlag.Paired) != 0)
            {
                paired++;
                readPair.Paired++;
            }
            if ((sequence.ReadFlag & ReadFlag.Aligned) != 0)
            {
                aligned++;
                alignedBases += (ulong)sequence.Read.Length;
                readPair.AlignedAndPaired++;
            }
            if ((sequence.ReadFlag & ReadFlag.AlignedAndPaired) == ReadFlag.AlignedAndPaired) alignedAndPaired++;
            if ((sequence.ReadFlag & ReadFlag.SegmentUnmapped) != 0) segmentUnmapped++;
            if ((sequence.ReadFlag & ReadFlag.NextSegmentUnmapped) != 0) nextSegmentUnmapped++;
            if ((sequence.ReadFlag & ReadFlag.ReverseComplemented) != 0) reverseComplemented++;
            if ((sequence.ReadFlag & ReadFlag.NextSegmentReverseComplemented) != 0) nextSegmentReverseComplemented++;
            if ((sequence.ReadFlag & ReadFlag.NotPrimaryAlignment) != 0) nonPrimaryAlignment++;
            if ((sequence.ReadFlag & ReadFlag.FailedQualityChecks) != 0) failedQualityChecks++;
            if ((sequence.ReadFlag & ReadFlag.OpticalDuplicate) != 0) opticalDuplicate++;
        }

        public void Reset()
        {
            sequenceCount = 0;
        }

        public object Data => new
        {
            sequenceCount,
            paired,
            aligned,
            alignedAndPaired,
            segmentUnmapped,
            nextSegmentUnmapped,
            reverseComplemented,
            nextSegmentReverseComplemented,
            nonPrimaryAlignment,
            failedQualityChecks,
            opticalDuplicate,
            alignedBases,
            averageReadLength = (double)baseCount / (double)sequenceCount,
            histogram = readLengthHistogram
                .Select((k, v) => new ulong[] { (ulong)k.Key, k.Value.Paired, k.Value.AlignedAndPaired })
                .OrderBy(a => a[0])
        };

        class ReadPair
        {
            public ulong AlignedAndPaired { get; set; }

            public ulong Paired { get; set; }
        }
    }
}