using System;
using System.Collections.Generic;
using System.Text;
using ProcessingImageSDK.MotionVectors;

namespace ProcessingImageSDK
{
    public class MotionVectorUtils
    {
        public static MotionVectorBase[,] getMotionVectorArray(ProcessingImage frame, int blockSize, int searchDistance)
        {
            MotionVectorBase[,] vectors = null;
            int sizeX = (frame.getSizeX() - searchDistance * 2) / blockSize;
            int sizeY = (frame.getSizeY() - searchDistance * 2) / blockSize;

            vectors = new MotionVectorBase[sizeY, sizeX];
            return vectors;
        }

        public static void blendMotionVectors(MotionVectorBase[,] first, MotionVectorBase[,] second, int startX)
        {
            try
            {
                for (int i = 0; i < second.GetLength(0); i++)
                {
                    for (int j = 0; j < second.GetLength(1); j++)
                    {
                        first[i, j + startX] = second[i, j];
                    }
                }
            }
            catch
            {
            }
        }
    }
}
