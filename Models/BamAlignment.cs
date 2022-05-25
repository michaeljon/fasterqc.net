using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Ovation.FasterQC.Net
{
    [SuppressMessage("Code style", "IDE1006", Justification = "Names correspond to BAM structure field names")]
    public class BamAlignment
    {
        public uint block_size { get; set; }

        public int refID { get; set; }

        public int pos { get; set; }

        public byte l_read_name { get; set; }

        public byte mapq { get; set; }

        public ushort bin { get; set; }

        public ushort n_cigar_op { get; set; }

        public ushort flag { get; set; }

        public uint l_seq { get; set; }

        public int next_refID { get; set; }

        public int next_pos { get; set; }

        public int tlen { get; set; }

        public byte[] read_name { get; set; } = null!;

        public uint[] cigar { get; set; } = null!;

        public byte[] seq { get; set; } = null!;

        public byte[] qual { get; set; } = null!;
    }

    [SuppressMessage("Code style", "IDE1006", Justification = "Names correspond to BAM structure field names")]
    public class BamOptionalElement
    {
        public char[] tag { get; set; }

        public char val_type { get; set; }

        public object value { get; set; } = null!;

        public BamOptionalElement(byte[] block, ref int offset)
        {
            tag = Encoding.ASCII.GetChars(block, offset, 2); offset += 2;
            val_type = Encoding.ASCII.GetChars(block, offset, 1)[0]; offset += 1;

            // consume the rest
            switch (val_type)
            {
                case 'A': offset += 1; break;

                // byte
                case 'c': offset += 1; break;
                case 'C': offset += 1; break;

                // short
                case 's': offset += 2; break;
                case 'S': offset += 2; break;

                // int
                case 'i': offset += 4; break;
                case 'I': offset += 4; break;

                // float
                case 'f': offset += 4; break;

                // null-terminated string
                case 'Z':
                    while (block[offset++] != 0) ;
                    break;

                // null-terminated hex digit pairs
                case 'H':
                    while (block[offset++] != 0) ;
                    break;

                // array of stuff
                case 'B':
                    var subtype = Encoding.ASCII.GetChars(block, offset, 1)[0]; offset += 1;
                    var length = BitConverter.ToUInt32(new Span<byte>(block, offset, 4)); offset += 4;

                    // consume the stuff
                    for (var element = 0; element < length; element++)
                    {
                        switch (subtype)
                        {
                            // byte
                            case 'c': offset += 1; break;
                            case 'C': offset += 1; break;

                            // short
                            case 's': offset += 2; break;
                            case 'S': offset += 2; break;

                            // int
                            case 'i': offset += 4; break;
                            case 'I': offset += 4; break;

                            // float
                            case 'f': offset += 4; break;
                        }
                    }

                    break;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("tag: ");
            sb.Append(tag[0]);
            sb.Append(tag[1]);

            sb.Append(", type: ");
            sb.Append(val_type);

            return sb.ToString();
        }
    }
}
