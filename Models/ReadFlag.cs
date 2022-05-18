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
        /// </summary>
        Aligned = 2,

        /// <summary>
        /// </summary>
        SegmentUnmapped = 4,

        /// <summary>
        /// </summary>
        NextSegmentUnmapped = 8,

        /// <summary>
        /// </summary>
        ReverseComplemented = 16,

        /// <summary>
        /// </summary>
        NextSegmentReverseComplemented = 32,

        /// <summary>
        /// </summary>
        FirstSegment = 64,

        /// <summary>
        /// </summary>
        LastSegment = 128,

        /// <summary>
        /// </summary>
        NotPrimaryAlignment = 256,

        /// <summary>
        /// </summary>
        FailedQualityChecks = 512,

        /// <summary>
        /// </summary>
        OpticalDuplicate = 1024,

        /// <summary>
        /// </summary>
        SupplementaryAlignment = 2048
    }
}