using System;
using System.IO;
using System.Text;
using static Ovation.FasterQC.Net.Utils.CliOptions;

namespace Ovation.FasterQC.Net
{
    public class FastqReader : AbstractReader
    {
        protected readonly StreamReader streamReader;

        private bool disposedValue;

        public FastqReader(string fastq, bool gzipped)
            : base(fastq, gzipped)
        {
            streamReader = new StreamReader(bufferedStream, Encoding.ASCII, false, bufferSize);
        }

        public override bool ReadSequence(out Sequence? sequence)
        {
            try
            {
                if (streamReader.EndOfStream == true)
                {
                    On(Settings.Verbose, () => Console.Error.WriteLine("End of stream"));
                    sequence = null;
                    return false;
                }

                var identifier = Encoding.ASCII.GetBytes(streamReader.ReadLine() ?? "");
                var read = Encoding.ASCII.GetBytes(streamReader.ReadLine() ?? "");
                var blank = Encoding.ASCII.GetBytes(streamReader.ReadLine() ?? "");
                var quality = Encoding.ASCII.GetBytes(streamReader.ReadLine() ?? "");

                sequence = new Sequence(0, identifier, read, blank, quality);
                sequencesRead++;
                return true;
            }
            catch (EndOfStreamException)
            {
                On(Settings.Verbose, () => Console.Error.WriteLine("End of stream"));
                sequence = null;
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    streamReader?.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}
