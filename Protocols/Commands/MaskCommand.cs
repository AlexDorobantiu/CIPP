using ProcessingImageSDK;

namespace CIPPProtocols.Commands
{
    public class MaskCommand : Command
    {
        public ProcessingImage processingImage;

        public MaskCommand(string pluginFullName, object[] arguments, ProcessingImage processingImage)
        {
            this.pluginFullName = pluginFullName;
            this.arguments = arguments;
            this.processingImage = processingImage;
        }
    }
}
