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
            this.taskType = TaskTypeEnum.filter;
            this.taken = false;
            this.finishedSuccessfully = false;
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
            result.join(subTask.result);

            subParts--;
            if (subParts == 0)
            {
                finishedSuccessfully = true;
            }
        }
    }
}
