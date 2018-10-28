using System;
using System.Collections.Generic;
using System.Text;

namespace CIPPProtocols
{
    public abstract class Command
    {
        public string pluginFullName;
        public object[] arguments;
    }
}
