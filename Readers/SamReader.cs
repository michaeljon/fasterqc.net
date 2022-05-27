using System;
using System.Globalization;
using System.IO;
using System.Text;
using static Ovation.FasterQC.Net.Utils.CliOptions;

namespace Ovation.FasterQC.Net
{
    public class SamReader : AbstractReader
    {
        private readonly StreamReader streamReader;

        private bool disposedValue;

        public SamReader(string sam, bool gzipped = true)
            : base(sam, gzipped)
        {
            streamReader = new StreamReader(bufferedStream, Encoding.ASCII, false, bufferSize);

            ConsumeHeader();
        }

        public override bool ReadSequence(out Sequence? sequence)
        {
            try
            {
                if (streamReader.EndOfStream == true)
                {
                    goto endofstream;
                }

                var entry = streamReader.ReadLine();
                if (entry == null)
                {
                    goto endofstream;
                }

                // this is clearly a bad approach, we're going to be allocating a
                // ton of small strings here, probably better to read the line,
                // find the tabs ourselves, then pull the bytes out of the components
                var parts = entry.Split('\t', StringSplitOptions.TrimEntries);

                var identifier = Encoding.ASCII.GetBytes(parts[0]);
                var flag = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
                var read = Encoding.ASCII.GetBytes(parts[9]);
                var blank = Encoding.ASCII.GetBytes("");
                var quality = Encoding.ASCII.GetBytes(parts[10]);

                sequence = new Sequence(flag, identifier, read, blank, quality);
                sequencesRead++;
                return true;
            }
            catch (EndOfStreamException)
            {
                goto endofstream;
            }

        endofstream:
            On(Settings.Verbose, () => Console.Error.WriteLine("End of stream"));
            sequence = null;
            return false;
        }

        private void ConsumeHeader()
        {
            try
            {
                while (streamReader.Peek() == '@')
                {
                    var header = streamReader.ReadLine();
                    On(Settings.Debug, () => Console.Error.WriteLine(header));
                }
            }
            catch (EndOfStreamException)
            {
                // swallow this, we've run out of file and a call
                // into ReadSequence will handle the EOF case
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    streamReader?.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}
