namespace Ovation.FasterQC.Net
{
    public class Adapter
    {
        public string Name { get; init; }
        public string Sequence { get; init; }
        public long[] Positions { get; private set; }

        public Adapter(string name, string sequence)
        {
            Name = name;
            Sequence = sequence;
            Positions = new long[1];
        }

        public void IncrementCount(int position)
        {
            ++Positions[position];
        }

        public void ExpandLengthTo(int newLength)
        {
            long[] newPositions = new long[newLength];
            for (int i = 0; i < Positions.Length; i++)
            {
                newPositions[i] = Positions[i];
            }

            if (Positions.Length > 0)
            {
                for (int i = Positions.Length; i < newPositions.Length; i++)
                {
                    newPositions[i] = Positions[^1];
                }
            }

            Positions = newPositions;
        }

        public long[] PetPositions()
        {
            return Positions;
        }
    }
}
