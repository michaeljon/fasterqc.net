namespace Ovation.FasterQC.Net
{
    public class NullProcessor : IQcModule
    {
        private ulong sequenceCount;

        public string Name => "nullProcessor";

        public string Description => "Runs a minimal processor";

        public bool IsEnabledForAll => false;

        public ReaderType SupportedReaders => ReaderType.AllReaders;

        public object Data => sequenceCount;

        public void ProcessSequence(Sequence sequence)
        {
            sequenceCount++;
        }

        public void Reset()
        {
            sequenceCount = 0;
        }
    }
}
