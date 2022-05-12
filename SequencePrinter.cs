using System;

namespace Ovation.FasterQC.Net
{
    public static class SequencePrinter
    {
        public static void PrintSequence(Sequence sequence)
        {
            var identifier = sequence.Identifier.ToArray();
            var readData = sequence.Read.ToArray();
            var blank = sequence.Blank.ToArray();
            var quality = sequence.Quality.ToArray();

            for (var id = 0; id < identifier.Length; id++)
            {
                Console.Write(Convert.ToChar(identifier[id]));
            }
            Console.WriteLine();

            for (var id = 0; id < readData.Length; id++)
            {
                Console.Write(Convert.ToChar(readData[id]));
            }
            Console.WriteLine();

            for (var id = 0; id < blank.Length; id++)
            {
                Console.Write(Convert.ToChar(blank[id]));
            }
            Console.WriteLine();

            for (var id = 0; id < quality.Length; id++)
            {
                Console.Write(Convert.ToChar(quality[id]));
            }
            Console.WriteLine();
        }
    }
}
