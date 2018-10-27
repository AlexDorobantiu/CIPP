using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ProcessingImageSDK.MotionVectors;

namespace CIPPProtocols.Tasks
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
            this.type = Type.MOTION_RECOGNITION;
            this.status = Status.NOT_TAKEN;
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
            if (status == Status.FAILED)
            {
                return;
            }
            if (subTask.status == Status.FAILED)
            {
                status = Status.FAILED;
                return;
            }

            int imagePosition = subTask.frame.getPositionX();
            if (imagePosition == 0)
            {
                MotionVectorUtils.blendMotionVectors(result, subTask.result, 0);
            }
            else
            {
                MotionVectorUtils.blendMotionVectors(result, subTask.result, (imagePosition - searchDistance) / blockSize);
            }

            subParts--;
            if (subParts == 0)
            {
                this.status = Status.SUCCESSFUL;
            }
        }
    }
}
