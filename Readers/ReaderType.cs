using System;

namespace Ovation.FasterQC.Net
{
    [Flags]
    public enum ReaderType
    {
        Fastq = 1,

        FastqGz = 2,

        UnalignedReaders = Fastq | FastqGz,

        Sam = 4,

        SamGz = 8,

        Bam = 16,

        AlignedReaders = Sam | SamGz | Bam,

        AllReaders = UnalignedReaders | AlignedReaders
    }
}