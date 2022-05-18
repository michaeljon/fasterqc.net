
namespace Ovation.FasterQC.Net
{
    public class AlignmentStatistics : IQcModule
    {
        private ulong sequenceCount;

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

        public string Name => "alignmentStatistics";

        public string Description => "Calculates alignment statistics for SAM/BAM files";

        public void ProcessSequence(Sequence sequence)
        {
            sequenceCount++;

            if ((sequence.ReadFlag & ReadFlag.Paired) != 0) paired++;
            if ((sequence.ReadFlag & ReadFlag.Aligned) != 0) aligned++;
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
            opticalDuplicate
        };
    }
}