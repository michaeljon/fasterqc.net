using System;
using Ovation.FasterQC.Net.Utils;

namespace Ovation.FasterQC.Net.Readers
{
    public static class ReaderFactory
    {
        public static ISequenceReader Create(CliOptions settings)
        {
            return settings switch
            {
                { Fastq: true } => new FastqLineReader(settings.InputFilename, true),
                { Bam: true } => new BamReader(settings.InputFilename),
                _ => throw new InvalidOperationException($"could not determine file type of {settings.InputFilename}")
            };
        }
    }
}
