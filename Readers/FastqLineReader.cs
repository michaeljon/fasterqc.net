using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using static Ovation.FasterQC.Net.Utils.CliOptions;

namespace Ovation.FasterQC.Net
{
    public class FastqLineReader : ISequenceReader
    {
        private readonly FileStream inputStream;

        private readonly GZipStream gzipStream;

        private readonly BufferedStream bufferedStream;

        private readonly StreamReader streamReader;

        private bool disposedValue;

        private int sequencesRead = 0;

        public int SequencesRead => sequencesRead;

        public double ApproximateCompletion =>
            100.0 * inputStream.Position / inputStream.Length;

        public FastqLineReader(string fastq, bool gzipped = true)
        {
            var bufferSize = 128 * 1024;

            var fileStreamOptions = new FileStreamOptions()
            {
                Mode = FileMode.Open,
                BufferSize = bufferSize,
            };

            if (gzipped == true)
            {
                inputStream = File.Open(fastq, fileStreamOptions);
                gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
                bufferedStream = new BufferedStream(gzipStream, bufferSize);
                streamReader = new StreamReader(bufferedStream, Encoding.ASCII, false, bufferSize);
            }
            else
            {
                inputStream = File.Open(fastq, fileStreamOptions);
                bufferedStream = new BufferedStream(inputStream, bufferSize);
                streamReader = new StreamReader(bufferedStream, Encoding.ASCII, false, bufferSize);
            }
        }

        public bool ReadSequence(out Sequence sequence)
        {
            try
            {
                if (streamReader.EndOfStream == true)
                {
                    On(Settings.Verbose, () => Console.Error.WriteLine("End of stream"));
                    sequence = null;
                    return false;
                }

                var identifier = Encoding.ASCII.GetBytes(streamReader.ReadLine());
                var read = Encoding.ASCII.GetBytes(streamReader.ReadLine());
                var blank = Encoding.ASCII.GetBytes(streamReader.ReadLine());
                var quality = Encoding.ASCII.GetBytes(streamReader.ReadLine());

                sequence = new Sequence(identifier, read, blank, quality);
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    streamReader?.Dispose();
                    bufferedStream?.Dispose();
                    gzipStream?.Dispose();
                    inputStream?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
