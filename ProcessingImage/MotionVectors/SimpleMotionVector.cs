using System;

namespace ProcessingImageSDK.MotionVectors
{
    /// <summary>
    /// Implements the basic motion vector
    /// </summary>
    [Serializable]
    public class SimpleMotionVector : MotionVectorBase
    {
        /// <summary>
        /// Initializes the vector with x and y values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public SimpleMotionVector(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
