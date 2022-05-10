using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace Ovation.FasterQC.Net
{
    class Program
    {
        private static readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        private static readonly List<IQcModule> modules = new()
        {
            new BasicStatistics(),
            new KMerContent(),
            new NCountsAtPosition(),
            new PerPositionSequenceContent(),
            new PerSequenceGcContent(),
            new QualityDistribution(),
            new SequenceLengthDistribution()
        };

        static void Main(string[] args)
        {
            using var inputStream = File.Open(args[0], FileMode.Open);
            using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            using var binaryReader = new BinaryReader(gzipStream);

            while (ReadSequence(binaryReader, out Sequence sequence))
            {
                foreach (var module in modules)
                {
                    module.ProcessSequence(sequence);
                }
            }

            var results = new Dictionary<string, object>();
            foreach (var module in modules)
            {
                results[module.Name] = module.Data;
            }
            Console.WriteLine(JsonSerializer.Serialize(results, options));
        }

        static bool ReadSequence(BinaryReader binaryReader, out Sequence sequence)
        {
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

        static void PrintSequence(Sequence sequence)
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
