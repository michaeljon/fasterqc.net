using System;
using Ovation.FasterQC.Net.Utils;

namespace Ovation.FasterQC.Net.Readers
{
    public static class ReaderFactory
    {
        public static ISequenceReader Create(CliOptions settings)
        {
            return settings.Format switch
            {
                ReaderType.Fastq => new FastqReader(settings.InputFilename, false),
                ReaderType.FastqGz => new FastqReader(settings.InputFilename, true),

                ReaderType.FastqLine => new FastqLineReader(settings.InputFilename, false),
                ReaderType.FastqLineGz => new FastqLineReader(settings.InputFilename, true),

                ReaderType.Sam => new SamReader(settings.InputFilename, false),
                ReaderType.SamGz => new SamReader(settings.InputFilename, true),

                ReaderType.Bam => new BamReader(settings.InputFilename),

                _ => throw new InvalidOperationException($"could not determine file type of {settings.InputFilename}")
            };
        }
    }
}
