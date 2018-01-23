using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using ParametersSDK;

namespace CIPPProtocols
{
    public class PluginInfo
    {
        public string displayName;
        public string fullName;
        public Assembly assembly;
        public Type type;
        public List<IParameters> parameters;

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
