using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;

namespace CIPPProtocols
{
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
            this.taskType = TaskTypeEnum.motionRecognition;
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
}
