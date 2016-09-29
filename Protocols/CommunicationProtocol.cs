using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;

namespace CIPPProtocols
{
    public class IDGenerator
    {
        private static int id = 0;
        private static object locker = new object();
        public static int getID()
        {
            lock (locker)
            {
                id++;
                return id;
            }
        }
    }

    public enum TrasmissionFlags
    {
        ClientName = 1,   //followed by machine name (string)
        TaskRequest = 2,
        Task = 3,         //followed by Task Package (TaskPackage)
        Result = 4,       //followed by Result Package (ResultPackage)
        AbortWork = 5,
        Listening = 6
    }

    public enum TaskType
    {
        filter,
        mask,
        motionRecognition
    }

    public enum GranularityType
    {
        low,
        medium,
        maximum
    }

    [Serializable]
    public abstract class Task
    {
        public int id;
        public TaskType taskType;
        public string pluginFullName;
        public object[] parameters;

        public bool taken;
        public bool state;
    }

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
            this.taskType = TaskType.filter;
            this.taken = false;
            this.state = false;
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
            if (subParts == 0) state = true;
        }
    }

    [Serializable]
    public class MaskTask : Task
    {
        public ProcessingImage originalImage;
        public byte[,] result;

        public MaskTask(int id, string pluginFullName, object[] parameters, ProcessingImage originalImage)
        {
            this.taskType = TaskType.mask;
            this.taken = false;
            this.state = false;

            this.id = id;
            this.pluginFullName = pluginFullName;
            this.parameters = parameters;
            this.originalImage = originalImage;
            this.result = null;
        }
    }

    [Serializable]
    public class MotionRecognitionTask : Task
    {
        [NonSerialized]
        public int motionId;

        public int blockSize;
        public int searchDistance;

        [NonSerialized]
        public int subParts;
        [NonSerialized]
        public MotionRecognitionTask parent;

        public ProcessingImage frame;
        public ProcessingImage nextFrame;

        [NonSerialized]
        public MotionVectorBase[,] result;

        public MotionRecognitionTask(int id, int motionId, int blockSize, int searchDistance, string pluginFullName, object[] parameters, ProcessingImage frame, ProcessingImage nextFrame)
        {
            this.taskType = TaskType.motionRecognition;
            this.id = id;
            this.motionId = motionId;
            this.blockSize = blockSize;
            this.searchDistance = searchDistance;
            this.pluginFullName = pluginFullName;
            this.parameters = parameters;
            this.frame = frame;
            this.nextFrame = nextFrame;

            this.subParts = 0;
            this.parent = null;
            this.result = null;
        }

        public void join(MotionRecognitionTask subTask)
        {
            int imagePosition = subTask.frame.getPositionX();
            if (imagePosition == 0)
            {
                MotionVectors.blendMotionVectors(result, subTask.result, 0);
            }
            else
            {
                MotionVectors.blendMotionVectors(result, subTask.result, (imagePosition - searchDistance) / blockSize);
            }

            subParts--;
            if (subParts == 0) state = true;
        }
    }        

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
