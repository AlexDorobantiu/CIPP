using System;

using ProcessingImageSDK;
using ProcessingImageSDK.MotionVectors;

namespace CIPPProtocols.Tasks
{
    [Serializable]
    public class MotionRecognitionTask : Task
    {
        [NonSerialized]
        public readonly int motionId;

        public readonly int blockSize;
        public readonly int searchDistance;

        [NonSerialized]
        public int subParts;
        [NonSerialized]
        public readonly MotionRecognitionTask parent;

        public readonly ProcessingImage frame;
        public readonly ProcessingImage nextFrame;

        [NonSerialized]
        public MotionVectorBase[,] result;

        public MotionRecognitionTask(int id, int motionId, int blockSize, int searchDistance, string pluginFullName, object[] parameters, 
            ProcessingImage frame, ProcessingImage nextFrame, MotionRecognitionTask parent)
            : base(id, Type.MOTION_RECOGNITION, pluginFullName, parameters)
        {
            status = Status.NOT_TAKEN;
            this.motionId = motionId;
            this.blockSize = blockSize;
            this.searchDistance = searchDistance;
            this.frame = frame;
            this.nextFrame = nextFrame;
            this.parent = parent;

            subParts = 0;
            result = null;
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

            int imagePosition = subTask.frame.getPosition().x;
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
                status = Status.SUCCESSFUL;
            }
        }

        public override object getResult()
        {
            return result;
        }
    }
}
