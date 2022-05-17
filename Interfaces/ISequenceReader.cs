using System;

namespace Ovation.FasterQC.Net
{
    public interface ISequenceReader : IDisposable
    {
        int SequencesRead { get; }

        bool ReadSequence(out Sequence sequence);

        double ApproximateCompletion { get; }
    }
}
