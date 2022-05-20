using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using static Ovation.FasterQC.Net.Utils.CliOptions;

namespace Ovation.FasterQC.Net
{
    public class BamReader : ISequenceReader
    {
        private readonly FileStream inputStream;

        private readonly GZipStream gzipStream;

        private readonly BufferedStream bufferedStream;

        private readonly BinaryReader binaryReader;

        private bool disposedValue;

        private ulong sequencesRead = 0;

        public ulong SequencesRead => sequencesRead;

        public BamReader(string bam)
        {
            var bufferSize = 128 * 1024;

            var fileStreamOptions = new FileStreamOptions()
            {
                Mode = FileMode.Open,
                BufferSize = bufferSize,
            };

            inputStream = File.Open(bam, fileStreamOptions);
            gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            bufferedStream = new BufferedStream(gzipStream, bufferSize);
            binaryReader = new BinaryReader(bufferedStream);

            ConsumeHeader();
        }

        public bool ReadSequence(out Sequence? sequence)
        {
            try
            {
                var bamAlignment = ReadSequence();

                sequence = new Sequence(bamAlignment);
                sequencesRead++;
                return true;
            }
            catch (EndOfStreamException)
            {
                sequence = null;
                return false;
            }
        }

        public double ApproximateCompletion =>
            100.0 * inputStream.Position / inputStream.Length;

        private void ConsumeHeader()
        {
            var magic = binaryReader.ReadBytes(4);
            var l_text = binaryReader.ReadUInt32();
            var text = binaryReader.ReadBytes((int)l_text);
            var n_ref = binaryReader.ReadUInt32();

            On(Settings.Debug, () =>
            {
                Console.Error.WriteLine($"magic: {(char)magic[0]}{(char)magic[1]}{(char)magic[2]}");
                Console.Error.WriteLine($"l_text: {l_text}");
                Console.Error.WriteLine($"text: ");
                Console.Error.WriteLine(new string(Encoding.ASCII.GetChars(text)));
                Console.Error.WriteLine($"n_ref: {n_ref}");
            });

            for (var refSeq = 0; refSeq < n_ref; refSeq++)
            {
                var l_name = binaryReader.ReadUInt32();

                // name is null-terminated, we don't want to read the null into
                // the string, but we do want to consume it from the stream
                var name = binaryReader.ReadBytes((int)l_name - 1); binaryReader.ReadByte();
                var l_ref = binaryReader.ReadUInt32();

                On(Settings.Debug, () =>
                {
                    Console.Error.WriteLine($"refSeq: {refSeq}");
                    Console.Error.WriteLine($"l_name: {l_name}");
                    Console.Error.WriteLine($"name: {new string(Encoding.ASCII.GetChars(name))}");
                    Console.Error.WriteLine($"l_ref: {l_ref}");
                });
            }
        }

        private BamAlignment ReadSequence()
        {
            char[] BASE_CODES = { '\0', 'A', 'C', 'M', 'G', 'R', 'S', 'V', 'T', 'W', 'Y', 'H', 'K', 'D', 'B', 'N' };

            var block_size = binaryReader.ReadUInt32();
            var block = binaryReader.ReadBytes((int)block_size);
            var offset = 0;

            var bamAlignment = new BamAlignment
            {
                block_size = block_size,
                refID = BitConverter.ToInt32(block, offset)
            };

            offset += 4;
            bamAlignment.pos = BitConverter.ToInt32(block, offset) + 1; offset += 4;
            bamAlignment.l_read_name = block[offset]; offset += 1;
            bamAlignment.mapq = block[offset]; offset += 1;
            bamAlignment.bin = BitConverter.ToUInt16(block, offset); offset += 2;
            bamAlignment.n_cigar_op = BitConverter.ToUInt16(block, offset); offset += 2;
            bamAlignment.flag = BitConverter.ToUInt16(block, offset); offset += 2;
            bamAlignment.l_seq = BitConverter.ToUInt32(block, offset); offset += 4;
            bamAlignment.next_refID = BitConverter.ToInt32(block, offset); offset += 4;
            bamAlignment.next_pos = BitConverter.ToInt32(block, offset) + 1; offset += 4;
            bamAlignment.tlen = BitConverter.ToInt32(block, offset); offset += 4;

            bamAlignment.read_name = new byte[bamAlignment.l_read_name - 1];
            Array.Copy(block, offset, bamAlignment.read_name, 0, bamAlignment.l_read_name - 1);
            offset += bamAlignment.l_read_name;

            bamAlignment.cigar = new uint[bamAlignment.n_cigar_op];
            Array.Copy(block, offset, bamAlignment.cigar, 0, bamAlignment.n_cigar_op);
            offset += bamAlignment.n_cigar_op * 4; // cigar_op is uint

            if (bamAlignment.l_seq != 0)
            {
                bamAlignment.seq = new byte[bamAlignment.l_seq];
                for (var b = 0; b < (bamAlignment.l_seq + 1) / 2; b++)
                {
                    var seq = block[b + offset];

                    bamAlignment.seq[(b * 2)] = (byte)BASE_CODES[(seq & 0xf0) >> 4];

                    // the very last pair might be null, don't write it
                    if ((seq & 0x0f) != 0)
                    {
                        bamAlignment.seq[(b * 2) + 1] = (byte)BASE_CODES[(seq & 0x0f)];
                    }
                }
            }
            else
            {
                bamAlignment.seq = Array.Empty<byte>();
            }
            offset += (int)((bamAlignment.l_seq + 1) / 2);

            if (bamAlignment.l_seq != 0)
            {
                bamAlignment.qual = new byte[(int)bamAlignment.l_seq];
                for (var q = 0; q < bamAlignment.l_seq; q++)
                {
                    block[q + offset] += 33;
                }
                Array.Copy(block, offset, bamAlignment.qual, 0, (int)bamAlignment.l_seq);
            }
            else
            {
                bamAlignment.qual = Array.Empty<byte>();
            }
            offset += (int)bamAlignment.l_seq;

            while (offset < block_size)
            {
                _ = new BamOptionalElement(block, ref offset);
            }

            return bamAlignment;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    binaryReader?.Dispose();
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
