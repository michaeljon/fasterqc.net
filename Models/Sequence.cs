using System;

namespace Ovation.FasterQC.Net
{
    public class Sequence
    {
        public ReadOnlyMemory<byte> Identifier { get; }

        public ReadOnlyMemory<byte> Read { get; }

        public ReadOnlyMemory<byte> Blank { get; }

        public ReadOnlyMemory<byte> Quality { get; }

        public Sequence(byte[] lines, int[] endOfLines)
        {
            Identifier = new ReadOnlyMemory<byte>(lines, 0, endOfLines[0]);
            Read = new ReadOnlyMemory<byte>(lines, endOfLines[0], endOfLines[1] - endOfLines[0]);
            Blank = new ReadOnlyMemory<byte>(lines, endOfLines[1], endOfLines[2] - endOfLines[1]);
            Quality = new ReadOnlyMemory<byte>(lines, endOfLines[2], endOfLines[3] - endOfLines[2]);
        }
    }
}