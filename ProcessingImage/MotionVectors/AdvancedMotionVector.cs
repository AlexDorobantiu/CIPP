using System;

namespace ProcessingImageSDK.MotionVectors
{
    /// <summary>
    /// Implements a motion vector with zoom ratio and planar rotation
    /// </summary>
    [Serializable]
    public class AdvancedMotionVector : MotionVectorBase
    {
        /// <summary>
        /// Zoom ratio
        /// </summary>
        public float zoom;

        /// <summary>
        /// Rotation angle
        /// </summary>
        public float angle;

        /// <summary>
        /// Initializer constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="zoom"></param>
        /// <param name="angle"></param>
        public AdvancedMotionVector(int x, int y, float zoom, float angle)
        {
            this.x = x;
            this.y = y;
            this.zoom = zoom;
            this.angle = angle;
        }
    }
}
