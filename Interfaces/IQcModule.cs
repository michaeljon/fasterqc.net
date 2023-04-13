
using System.Collections.Generic;

namespace Ovation.FasterQC.Net
{
    public interface IQcModule<TM, TR>
    {
        string Name { get; }

        string Description { get; }

        bool IsEnabledForAll { get; }

        ReaderType SupportedReaders { get; }

        TM Map(IEnumerable<Sequence> sequences);

        TR Reduce(IEnumerable<TM> mapResults);
    }
}
