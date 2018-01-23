using System;
using System.Collections.Generic;
using System.Text;
using ProcessingImageSDK;

namespace CIPPProtocols.Commands
{
    public class FilterCommand
    {
        public string pluginFullName;
        public object[] arguments;
        public ProcessingImage processingImage;

        public FilterCommand(string pluginFullName, object[] arguments, ProcessingImage processingImage)
        {
            this.pluginFullName = pluginFullName;
            this.arguments = arguments;
            this.processingImage = processingImage;
        }
    }
}
