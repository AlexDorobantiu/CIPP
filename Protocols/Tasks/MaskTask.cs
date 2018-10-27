using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;

namespace CIPPProtocols.Tasks
{
    [Serializable]
    public class MaskTask : Task
    {
        public ProcessingImage originalImage;
        public byte[,] result;

        public MaskTask(int id, string pluginFullName, object[] parameters, ProcessingImage originalImage)
        {
            this.type = Type.MASK;
            this.status = Status.NOT_TAKEN;

            this.id = id;
            this.pluginFullName = pluginFullName;
            this.parameters = parameters;
            this.originalImage = originalImage;
            this.result = null;
        }
    }
}
