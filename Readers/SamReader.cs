using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using static Ovation.FasterQC.Net.Utils.CliOptions;

namespace Ovation.FasterQC.Net
{
    public class SamReader : ISequenceReader
    {
        private readonly FileStream inputStream;

        private readonly GZipStream? gzipStream;

        private readonly BufferedStream bufferedStream;

        private readonly StreamReader streamReader;

        private bool disposedValue;

        private ulong sequencesRead = 0;

        public ulong SequencesRead => sequencesRead;

        public SamReader(string sam, bool gzipped = true)
        {
            var bufferSize = 128 * 1024;

            var fileStreamOptions = new FileStreamOptions()
            {
                Mode = FileMode.Open,
                BufferSize = bufferSize,
            };

            if (gzipped == true)
            {
                inputStream = File.Open(sam, fileStreamOptions);
                gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
                bufferedStream = new BufferedStream(gzipStream, bufferSize);
                streamReader = new StreamReader(bufferedStream, Encoding.ASCII, false, bufferSize);
            }
            else
            {
                inputStream = File.Open(sam, fileStreamOptions);
                bufferedStream = new BufferedStream(inputStream, bufferSize);
                streamReader = new StreamReader(bufferedStream, Encoding.ASCII, false, bufferSize);
            }

            ConsumeHeader();
        }

        private void ConsumeHeader()
        {
            try
            {
                while (streamReader.Peek() == '@')
                {
                    var header = streamReader.ReadLine();
                    On(Settings.Debug, () => Console.Error.WriteLine(header));
                }
            }
            catch (EndOfStreamException)
            {
                // swallow this, we've run out of file and a call
                // into ReadSequence will handle the EOF case
            }
        }

        public bool ReadSequence(out Sequence? sequence)
        {
            try
            {
                if (streamReader.EndOfStream == true)
                {
                    goto endofstream;
                }

                var entry = streamReader.ReadLine();
                if (entry == null)
                {
                    goto endofstream;
                }

                // this is clearly a bad approach, we're going to be allocating a
                // ton of small strings here, probably better to read the line,
                // find the tabs ourselves, then pull the bytes out of the components
                var parts = entry.Split('\t', StringSplitOptions.TrimEntries);

                var identifier = Encoding.ASCII.GetBytes(parts[0]);
                var flag = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
                var read = Encoding.ASCII.GetBytes(parts[9]);
                var blank = Encoding.ASCII.GetBytes("");
                var quality = Encoding.ASCII.GetBytes(parts[10]);

                sequence = new Sequence(flag, identifier, read, blank, quality);
                sequencesRead++;
                return true;
            }
            catch (EndOfStreamException)
            {
                goto endofstream;
            }

        endofstream:
            On(Settings.Verbose, () => Console.Error.WriteLine("End of stream"));
            sequence = null;
            return false;
        }

        public double ApproximateCompletion =>
            100.0 * inputStream.Position / inputStream.Length;

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
