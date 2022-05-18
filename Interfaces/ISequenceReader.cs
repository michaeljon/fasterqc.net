using System;

namespace Ovation.FasterQC.Net
{
    public interface ISequenceReader : IDisposable
    {
        ulong SequencesRead { get; }

        bool ReadSequence(out Sequence? sequence);

        double ApproximateCompletion { get; }
    }
}
