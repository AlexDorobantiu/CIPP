using ProcessingImageSDK;

namespace CIPPProtocols.Commands
{
    public class FilterCommand : Command
    {
        public ProcessingImage processingImage;

        public FilterCommand(string pluginFullName, object[] arguments, ProcessingImage processingImage)
        {
            this.pluginFullName = pluginFullName;
            this.arguments = arguments;
            this.processingImage = processingImage;
        }
    }
}
