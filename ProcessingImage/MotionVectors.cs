using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK
{
    public enum MotionVectorType
    {
        simple,
        advanced,
        depth
    }

    [Serializable]
    public abstract class MotionVectorBase
    {
        public int x;
        public int y;
    }

    [Serializable]
    public class SimpleMotionVector : MotionVectorBase
    {
        public SimpleMotionVector(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [Serializable]
    public class AdvancedMotionVector : MotionVectorBase
    {
        public float zoom;
        public float angle;

        public AdvancedMotionVector(int x, int y, float zoom, float angle)
        {
            this.x = x;
            this.y = y;
            this.zoom = zoom;
            this.angle = angle;
        }
    }

    [Serializable]
    public class DepthMotionVector : MotionVectorBase
    {
        public float zoom;
        public float angleX;
        public float angleY;
        public float angleZ;

        public DepthMotionVector(int x, int y, float zoom, float angleX, float angleY, float angleZ)
        {
            this.x = x;
            this.y = y;
            this.zoom = zoom;
            this.angleX = angleX;
            this.angleY = angleY;
            this.angleZ = angleZ;
        }
    }

    public class MotionVectors
    {
        public static MotionVectorBase[,] getMotionVectorArray(ProcessingImage frame, int blockSize, int searchDistance)
        {
            MotionVectorBase[,] vectors = null;
            int sizeX = (frame.getSizeX() - searchDistance * 2) / blockSize;
            int sizeY = (frame.getSizeY() - searchDistance * 2) / blockSize;

            vectors = new MotionVectorBase[sizeY, sizeX];
            //switch (motionVectorType)
            //{
            //    case MotionVectorType.simple:
            //        {
            //            vectors = new SimpleMotionVector[sizeY, sizeX];
            //        } break;
            //    case MotionVectorType.advanced:
            //        {
            //            vectors = new AdvancedMotionVector[sizeY, sizeX];
            //        } break;
            //    case MotionVectorType.depth:
            //        {
            //            vectors = new DepthMotionVector[sizeY, sizeX];
            //        } break;
            //}
            return vectors;
        }

        public static void blendMotionVectors(MotionVectorBase[,] a, MotionVectorBase[,] b, int startX)
        {
            try
            {
                for (int i = 0; i < b.GetLength(0); i++)
                {
                    for (int j = 0; j < b.GetLength(1); j++)
                    {
                        a[i, j + startX] = b[i, j];
                    }
                }
            }
            catch
            {
            }
        }
    }
}
