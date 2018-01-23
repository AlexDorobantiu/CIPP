using System;
using System.Collections.Generic;
using System.Text;
using ProcessingImageSDK;

namespace CIPPProtocols.Commands
{
    public class MotionRecognitionCommand
    {
        public string pluginFullName;
        public object[] arguments;
        public List<ProcessingImage> processingImageList;

        public MotionRecognitionCommand(string pluginFullName, object[] arguments, List<ProcessingImage> processingImageList)
        {
            this.pluginFullName = pluginFullName;
            this.arguments = arguments;
            this.processingImageList = processingImageList;
        }
    }
}
