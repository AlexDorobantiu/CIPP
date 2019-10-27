using ProcessingImageSDK.MotionVectors;

namespace ProcessingImageSDK
{
    /// <summary>
    /// Utilities for working with motion vectors
    /// </summary>
    public class MotionVectorUtils
    {
        /// <summary>
        /// Computes the size of the motion vector matrix and returns an empty matrix of that size.
        /// </summary>
        /// <param name="frame">Image for which the motion vectors are computed</param>
        /// <param name="blockSize">Size of the block for which a motion vector is computed</param>
        /// <param name="searchDistance">Distance for which the best block match is search for</param>
        /// <returns></returns>
        public static MotionVectorBase[,] getMotionVectorArray(ProcessingImage frame, int blockSize, int searchDistance)
        {
            MotionVectorBase[,] vectors = null;
            int sizeX = (frame.getSizeX() - searchDistance * 2) / blockSize;
            int sizeY = (frame.getSizeY() - searchDistance * 2) / blockSize;

            vectors = new MotionVectorBase[sizeY, sizeX];
            return vectors;
        }

        /// <summary>
        /// Joins twe second motion vector matrix into the first at the startX index
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="startX"></param>
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
