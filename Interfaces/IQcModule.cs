
namespace Ovation.FasterQC.Net
{
    public interface IQcModule
    {
        string Name { get; }

        string Description { get; }

        bool IsEnabledForAll { get; }

        ReaderType SupportedReaders { get; }

        void ProcessSequence(Sequence sequence);

        void Reset();

        object Data { get; }
    }
}