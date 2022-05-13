using System;
using System.Text;

namespace Ovation.FasterQC.Net
{
    public class Sequence
    {
        public byte[] Identifier { get; }

        public byte[] Read { get; }

        public byte[] Blank { get; }

        public byte[] Quality { get; }

        public Sequence(byte[] lines, int[] endOfLines)
        {
            Identifier = new ReadOnlyMemory<byte>(lines, 0, endOfLines[0]).ToArray();
            Read = new ReadOnlyMemory<byte>(lines, endOfLines[0], endOfLines[1] - endOfLines[0]).ToArray();
            Blank = new ReadOnlyMemory<byte>(lines, endOfLines[1], endOfLines[2] - endOfLines[1]).ToArray();
            Quality = new ReadOnlyMemory<byte>(lines, endOfLines[2], endOfLines[3] - endOfLines[2]).ToArray();
        }

        public Sequence(byte[] identifer, byte[] read, byte[] blank, byte[] quality)
        {
            Identifier = new ReadOnlyMemory<byte>(identifer).ToArray();
            Read = new ReadOnlyMemory<byte>(read).ToArray();
            Blank = new ReadOnlyMemory<byte>(blank).ToArray();
            Quality = new ReadOnlyMemory<byte>(quality).ToArray();
        }

        public Sequence(BamAlignment bamAlignment)
        {
            Identifier = new ReadOnlyMemory<byte>(bamAlignment.read_name).ToArray();
            Read = new ReadOnlyMemory<byte>(bamAlignment.seq).ToArray();
            Quality = new ReadOnlyMemory<byte>(bamAlignment.qual).ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder("sequence: \n");

            sb.AppendLine(new string(Encoding.ASCII.GetChars(Identifier)));
            sb.AppendLine(new string(Encoding.ASCII.GetChars(Read)));
            sb.AppendLine(new string(Encoding.ASCII.GetChars(Blank)));
            sb.AppendLine(new string(Encoding.ASCII.GetChars(Quality)));

            return sb.ToString();
        }
    }
}