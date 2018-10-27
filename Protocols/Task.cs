using System;
using System.Collections.Generic;
using System.Text;

namespace CIPPProtocols
{
    [Serializable]
    public abstract class Task
    {
        public enum Type
        {
            FILTER,
            MASK,
            MOTION_RECOGNITION
        }

        public enum Status
        {
            NOT_TAKEN,
            TAKEN,
            SUCCESSFUL,
            FAILED
        }
        
        public int id;
        public Type type;
        public string pluginFullName;
        public object[] parameters;
        public Status status;
        public Exception exception;
    }
}
