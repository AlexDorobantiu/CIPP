using System;

namespace ProcessingImageSDK.MotionVectors
{
    /// <summary>
    /// Models the minimum motion vector for an image block
    /// </summary>
    [Serializable]
    public abstract class MotionVectorBase
    {
        /// <summary>
        /// Models displacement on the horizontal axis
        /// </summary>
        public int x;

        /// <summary>
        /// Models displacement on the vertical axis
        /// </summary>
        public int y;
    }
}
