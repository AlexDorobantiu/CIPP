using System;

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

        public readonly int id;
        public readonly Type type;
        public readonly string pluginFullName;
        public readonly object[] parameters;
        public Status status;
        public Exception exception;

        protected Task(int id, Type type, string pluginFullName, object[] parameters)
        {
            this.id = id;
            this.type = type;
            this.pluginFullName = pluginFullName;
            this.parameters = parameters;
        }

        public abstract object getResult();

    }
}
