using System;
using System.Collections.Generic;
using System.Text;

namespace CIPPProtocols
{
    [Serializable]
    public class ResultPackage
    {
        public int taskId;
        public object result;

        public ResultPackage(int taskId, object result)
        {
            this.taskId = taskId;
            this.result = result;
        }
    }
}
