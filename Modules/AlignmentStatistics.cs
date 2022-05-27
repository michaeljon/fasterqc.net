
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

        private ulong firstSegment;

        private ulong lastSegment;

        private ulong embeddedSegment;

        private ulong unknownIndex;

        private ulong primaryAlignment;

        private ulong secondaryAlignment;

        private ulong nonPrimaryAlignment;

        private ulong failedQualityChecks;

        private ulong opticalDuplicate;

        private ulong alignedBases;

        private readonly Dictionary<int, ReadPair> readLengthHistogram = new();

        public string Name => "alignmentStatistics";

        public bool IsEnabledForAll => true;

        public ReaderType SupportedReaders => ReaderType.AlignedReaders;

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

            // revisit the logic here based on https://samtools.github.io/hts-specs/SAMv1.pdf
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

            if ((sequence.ReadFlag & ReadFlag.FirstSegment) != 0) firstSegment++;
            if ((sequence.ReadFlag & ReadFlag.LastSegment) != 0) lastSegment++;
            if ((sequence.ReadFlag & (ReadFlag.EmbeddedSegment)) == ReadFlag.EmbeddedSegment) embeddedSegment++;
            if ((sequence.ReadFlag & (ReadFlag.EmbeddedSegment)) == 0) unknownIndex++;

            if ((sequence.ReadFlag & ReadFlag.NotPrimaryAlignment) != 0) nonPrimaryAlignment++;
            if ((sequence.ReadFlag & ReadFlag.FailedQualityChecks) != 0) failedQualityChecks++;
            if ((sequence.ReadFlag & ReadFlag.OpticalDuplicate) != 0) opticalDuplicate++;
            if ((sequence.ReadFlag & ReadFlag.SecondaryAlignment) == 0)
            {
                primaryAlignment++;
            }
            if ((sequence.ReadFlag & ReadFlag.SecondaryAlignment) == ReadFlag.SecondaryAlignment)
            {
                secondaryAlignment++;
            }
        }

        public void Reset()
        {
            sequenceCount = 0;
        }

        public object Data
        {
            get
            {
                var minReadLength = readLengthHistogram.Keys.Min();
                var maxReadLength = readLengthHistogram.Keys.Max();

                for (var readLength = minReadLength; readLength < maxReadLength; readLength++)
                {
                    if (readLengthHistogram.ContainsKey(readLength) == false)
                    {
                        readLengthHistogram.Add(readLength, new ReadPair());
                    }
                }

                return new
                {
                    sequenceCount,
                    paired,
                    aligned,
                    alignedAndPaired,
                    segmentUnmapped,
                    nextSegmentUnmapped,
                    reverseComplemented,
                    nextSegmentReverseComplemented,
                    firstSegment,
                    lastSegment,
                    embeddedSegment,
                    unknownIndex,
                    primaryAlignment,
                    secondaryAlignment,
                    nonPrimaryAlignment,
                    failedQualityChecks,
                    opticalDuplicate,
                    alignedBases,
                    averageReadLength = (double)baseCount / (double)sequenceCount,
                    histogram = new
                    {
                        minReadLength,
                        maxReadLength,
                        paired = readLengthHistogram
                            .OrderBy(k => k.Key)
                            .Select((k, v) => k.Value.Paired),
                        unpaired = readLengthHistogram
                            .OrderBy(k => k.Key)
                            .Select((k, v) => k.Value.AlignedAndPaired)
                    }
                };
            }
        }

        class ReadPair
        {
            public ulong Paired { get; set; }

            public ulong AlignedAndPaired { get; set; }
        }
    }
}