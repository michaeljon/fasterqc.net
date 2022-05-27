using System;
using System.IO;
using System.IO.Compression;

namespace Ovation.FasterQC.Net
{
    public abstract class AbstractReader : ISequenceReader
    {
        private readonly FileStream inputStream;

        private readonly GZipStream? gzipStream;

        protected readonly BufferedStream bufferedStream;

        protected readonly int bufferSize = 128 * 1024;

        private bool disposedValue;

        protected ulong sequencesRead = 0;

        public ulong SequencesRead => sequencesRead;

        public double ApproximateCompletion =>
            100.0 * inputStream.Position / inputStream.Length;

        public AbstractReader(string filename, bool gzipped = true)
        {
            var fileStreamOptions = new FileStreamOptions()
            {
                Mode = FileMode.Open,
                BufferSize = bufferSize,
            };

            if (gzipped == true)
            {
                inputStream = File.Open(filename, fileStreamOptions);
                gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
                bufferedStream = new BufferedStream(gzipStream, bufferSize);
            }
            else
            {
                inputStream = File.Open(filename, fileStreamOptions);
                bufferedStream = new BufferedStream(inputStream, bufferSize);
            }
        }

        public abstract bool ReadSequence(out Sequence? sequence);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
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
