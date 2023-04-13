using System;
using System.IO;

namespace Ovation.BgzMR
{
    public class FileSliceStream : Stream
    {
        private readonly FileStream fileStream;

        private long bytesLeft;

        public FileSliceStream(string filePath, long startPos, long endPos)
        {
            fileStream = File.OpenRead(filePath);
            fileStream.Seek(startPos, SeekOrigin.Begin);
            bytesLeft = endPos - startPos;
        }


        public override void Flush()
        {
            fileStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (bytesLeft <= 0)
            {
                return 0;
            }

            var bytesRead = fileStream.Read(buffer, offset, count);
            if (bytesRead > bytesLeft)
            {
                bytesRead = (int)bytesLeft;
            }

            bytesLeft -= bytesRead;

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException("File slices must be read from start to end without seeking");
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("File slices do not support writing");
        }

        public override bool CanRead => fileStream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotImplementedException();

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}
