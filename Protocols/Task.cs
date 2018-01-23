using System;
using System.Collections.Generic;
using System.Text;

namespace CIPPProtocols
{
    [Serializable]
    public abstract class Task
    {
        public int id;
        public TaskTypeEnum taskType;
        public string pluginFullName;
        public object[] parameters;

        public bool taken;
        public bool finishedSuccessfully;
    }
}
