namespace Ovation.FasterQC.Net
{
    public class PolyX : IQcModule
    {
        private const byte POLY_SIZE = 10;

        private readonly ulong[] polyA = new ulong[2];

        private readonly ulong[] polyT = new ulong[2];

        private readonly ulong[] polyC = new ulong[2];

        private readonly ulong[] polyG = new ulong[2];

        private ulong sequenceCount;

        private ulong tooShort;

        public string Name => "polyX";

        public string Description => "Calculates the the number of reads with polyX heads / tails";

        public bool IsEnabledForAll => true;

        public object Data
        {
            get
            {
                return new
                {
                    sequenceCount,
                    tooShort,
                    target = POLY_SIZE,
                    head = new { a = polyA[0], t = polyT[0], c = polyC[0], g = polyG[0] },
                    tail = new { a = polyA[1], t = polyT[1], c = polyC[1], g = polyG[1] },
                };
            }
        }

        public void ProcessSequence(Sequence sequence)
        {
            sequenceCount++;

            if (sequence.Read.Length < POLY_SIZE)
            {
                tooShort++;
                return;
            }

            if (CheckPolyHead((byte)'A', sequence.Read) == true) polyA[0]++;
            if (CheckPolyTail((byte)'A', sequence.Read) == true) polyA[1]++;

            if (CheckPolyHead((byte)'T', sequence.Read) == true) polyT[0]++;
            if (CheckPolyTail((byte)'T', sequence.Read) == true) polyT[1]++;

            if (CheckPolyHead((byte)'C', sequence.Read) == true) polyC[0]++;
            if (CheckPolyTail((byte)'C', sequence.Read) == true) polyC[1]++;

            if (CheckPolyHead((byte)'G', sequence.Read) == true) polyG[0]++;
            if (CheckPolyTail((byte)'G', sequence.Read) == true) polyG[1]++;
        }

        public void Reset()
        {
            polyA[0] = 0; polyA[1] = 0;
            polyT[0] = 0; polyT[1] = 0;
            polyC[0] = 0; polyC[1] = 0;
            polyG[0] = 0; polyG[1] = 0;

            sequenceCount = 0;
            tooShort = 0;
        }

        private static bool CheckPolyHead(byte test, byte[] sequence)
        {
            // consider a loop unroll here
            for (var i = 0; i < POLY_SIZE; i++)
            {
                if (sequence[i] != test)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckPolyTail(byte test, byte[] sequence)
        {
            var endOfRead = sequence.Length - 1;
            var endOfPoly = sequence.Length - (POLY_SIZE + 1);

            // consider a loop unroll here
            for (var i = endOfRead; i > endOfPoly; i--)
            {
                if (sequence[i] != test)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
