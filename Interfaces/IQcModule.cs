
namespace Ovation.FasterQC.Net
{
    public interface IQcModule
    {
        string Name { get; }

        string Description { get; }

        void ProcessSequence(Sequence sequence);

        void Reset();

        object Data { get; }
    }
}