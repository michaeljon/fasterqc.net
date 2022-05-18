using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ovation.FasterQC.Net.Utils;

namespace Ovation.FasterQC.Net.Modules
{
    public static class ModuleFactory
    {
        private static Dictionary<string, IQcModule>? moduleMap;

        public static Dictionary<string, IQcModule> ModuleMap
        {
            get
            {
                if (moduleMap == null)
                {
                    moduleMap = new Dictionary<string, IQcModule>();

                    var modules = Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => string.IsNullOrEmpty(t.Namespace) == false && t.GetInterface(nameof(IQcModule)) != null)
                        .Select(t => Activator.CreateInstance(t))
                        .Cast<IQcModule>();

                    foreach (var module in modules)
                    {
                        moduleMap.Add(module.GetType().Name, module);
                    }
                }

                return moduleMap;
            }
        }

        public static IEnumerable<IQcModule> Create(CliOptions settings)
        {
            if (settings.ModuleNames.Any() == false || settings.ModuleNames.First() == "all")
            {
                var moduleNames = new List<string>();
                var modules = new List<IQcModule>();

                foreach (var module in ModuleMap.Where(m => m.Value.IsEnabledForAll == true))
                {
                    moduleNames.Add(module.Key);
                    modules.Add(module.Value);
                }

                settings.ModuleNames = moduleNames;
                return modules;
            }
            else
            {
                return settings.ModuleNames.Select(n => ModuleMap[n]);
            }
        }
    }
}
