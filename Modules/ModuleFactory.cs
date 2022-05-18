using System.Collections.Generic;
using System.Linq;
using Ovation.FasterQC.Net.Utils;

namespace Ovation.FasterQC.Net.Modules
{
    public static class ModuleFactory
    {
        private static readonly Dictionary<string, IQcModule> moduleMap = new()
        {
            ["AlignmentStatistics"] = new AlignmentStatistics(),
            ["BasicStatistics"] = new BasicStatistics(),
            ["KMerContent"] = new KMerContent(),
            ["NCountsAtPosition"] = new NCountsAtPosition(),
            ["PerPositionSequenceContent"] = new PerPositionSequenceContent(),
            ["PerSequenceGcContent"] = new PerSequenceGcContent(),
            ["QualityDistributionByBase"] = new QualityDistributionByBase(),
            ["MeanQualityDistribution"] = new MeanQualityDistribution(),
            ["SequenceLengthDistribution"] = new SequenceLengthDistribution(),
            ["PerPositionQuality"] = new PerPositionQuality(),
        };

        public static IEnumerable<IQcModule> Create(CliOptions settings)
        {
            if (settings.ModuleNames.Any() == false || settings.ModuleNames.First() == "all")
            {
                settings.ModuleNames = moduleMap.Keys;
                return moduleMap.Values;
            }
            else
            {
                return settings.ModuleNames.Select(n => moduleMap[n]);
            }
        }
    }
}
