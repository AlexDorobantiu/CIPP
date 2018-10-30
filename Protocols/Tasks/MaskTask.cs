using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;

namespace CIPPProtocols.Tasks
{
    [Serializable]
    public class MaskTask : Task
    {
        public readonly ProcessingImage originalImage;
        public byte[,] result;

        public MaskTask(int id, string pluginFullName, object[] parameters, ProcessingImage originalImage)
            : base(id, Type.MASK, pluginFullName, parameters)
        {
            this.status = Status.NOT_TAKEN;
            this.originalImage = originalImage;
            this.result = null;
        }

        public override object getResult()
        {
            return result;
        }
    }
}
