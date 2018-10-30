﻿using System;
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
        public readonly FilterTask parent;

        public readonly ProcessingImage originalImage;

        [NonSerialized]
        public ProcessingImage result;

        public FilterTask(int id, string pluginFullName, object[] parameters, ProcessingImage originalImage, FilterTask parent)
            : base(id, Type.FILTER, pluginFullName, parameters)
        {           
            this.status = Status.NOT_TAKEN;
            subParts = 0;
            this.parent = parent;
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

        public override object getResult()
        {
            return result;
        }
    }
}
