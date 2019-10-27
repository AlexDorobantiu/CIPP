using System;
using System.Collections.Generic;
using System.Reflection;
using ParametersSDK;

namespace CIPPProtocols.Plugin
{
    public class PluginInfo
    {
        public readonly string displayName;
        public readonly string fullName;
        public readonly Assembly assembly;
        public readonly Type type;
        public readonly List<IParameters> parameters;

        public PluginInfo(string displayName, string fullName, Assembly assembly, Type type, List<IParameters> parameters)
        {
            this.displayName = displayName;
            this.fullName = fullName;
            this.assembly = assembly;
            this.type = type;
            this.parameters = parameters;
        }
    }
}
