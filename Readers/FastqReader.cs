using System;
using System.IO;
using System.IO.Compression;

namespace Ovation.FasterQC.Net
{
    public class FastqReader : ISequenceReader
    {
        private readonly FileStream inputStream;

        private readonly GZipStream gzipStream;

        private readonly BinaryReader binaryReader;

        private bool disposedValue;

        public FastqReader(string fastq)
        {
            inputStream = File.Open(fastq, FileMode.Open);
            gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            binaryReader = new BinaryReader(gzipStream);
        }

        public bool ReadSequence(out Sequence sequence)
        {
            // this is clearly dangerous, instead read a large chunk of the file
            // and then walk through it returning only the consumed portion while
            // keeping track of the last byte consumed on the stream
            byte[] bytes = new byte[1024];

            int offset = 0;
            int line = 0;
            int[] endOfLines = new int[4];

            try
            {
                while (line < 4)
                {
                    var b = binaryReader.ReadByte();
                    if (b == (byte)'\n')
                    {
                        endOfLines[line++] = offset;
                    }
                    else
                    {
                        bytes[offset++] = b;
                    }
                }

                for (var read = endOfLines[1]; read < endOfLines[2]; read++)
                {
                    bytes[read] &= 0xdf;
                }

                sequence = new Sequence(bytes, endOfLines);
                return true;
            }
            catch (EndOfStreamException)
            {
                Console.Error.WriteLine("End of stream");
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
                    binaryReader?.Dispose();
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