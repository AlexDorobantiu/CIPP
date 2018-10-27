using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;

namespace CIPPProtocols.Tasks
{
    [Serializable]
    public class FilterTask : Task
    {
        [NonSerialized]
        public int subParts;

        [NonSerialized]
        public FilterTask parent;

        public ProcessingImage originalImage;

        [NonSerialized]
        public ProcessingImage result;

        public FilterTask(int id, string pluginFullName, object[] parameters, ProcessingImage originalImage)
        {
            this.type = Type.FILTER;
            this.status = Status.NOT_TAKEN;
            subParts = 0;
            parent = null;

            this.id = id;
            this.pluginFullName = pluginFullName;
            this.parameters = parameters;
            this.originalImage = originalImage;
            this.result = null;
        }

        public void join(FilterTask subTask)
        {
            if (status == Status.FAILED)
            {
                return;
            }
            if (subTask.status == Status.FAILED)
            {
                status = Status.FAILED;
                return;
            }

            result.join(subTask.result);

            subParts--;
            if (subParts == 0)
            {
                this.status = Status.SUCCESSFUL;
            }
        }
    }
}
