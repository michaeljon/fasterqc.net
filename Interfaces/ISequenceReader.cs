using System;

namespace Ovation.FasterQC.Net
{
    public interface ISequenceReader : IDisposable
    {
        bool ReadSequence(out Sequence sequence);
    }
}