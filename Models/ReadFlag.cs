using System;

namespace Ovation.FasterQC.Net
{
    [Flags]
    public enum ReadFlag : ushort
    {
        /// <summary>
        /// template having multiple templates in sequencing (read is paired)
        /// </summary>
        Paired = 1,

        /// <summary>
        /// each segment properly aligned according to the aligner (read mapped in proper pair)
        /// </summary>
        Aligned = 2,

        AlignedAndPaired = Aligned | Paired,

        /// <summary>
        /// segment unmapped (read1 unmapped)
        /// </summary>
        SegmentUnmapped = 4,

        /// <summary>
        /// next segment in the template unmapped (read2 unmapped)
        /// </summary>
        NextSegmentUnmapped = 8,

        /// <summary>
        /// SEQ being reverse complemented (read1 reverse complemented)
        /// </summary>
        ReverseComplemented = 16,

        /// <summary>
        /// SEQ of the next segment in the template being reverse complemented (read2 reverse complemented)
        /// </summary>
        NextSegmentReverseComplemented = 32,

        /// <summary>
        /// the first segment in the template (is read1)
        /// </summary>
        FirstSegment = 64,

        /// <summary>
        /// the last segment in the template (is read2)
        /// </summary>
        LastSegment = 128,

        /// <summary>
        /// not primary alignment
        /// </summary>
        NotPrimaryAlignment = 256,

        /// <summary>
        /// alignment fails quality checks
        /// </summary>
        FailedQualityChecks = 512,

        /// <summary>
        /// PCR or optical duplicate
        /// </summary>
        OpticalDuplicate = 1024,

        /// <summary>
        /// supplementary alignment (e.g. aligner specific, could be a portion of a split read or a tied region)
        /// </summary>
        SupplementaryAlignment = 2048
    }
}