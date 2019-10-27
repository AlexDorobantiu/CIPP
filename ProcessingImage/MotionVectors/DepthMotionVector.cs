using System;

namespace ProcessingImageSDK.MotionVectors
{
    /// <summary>
    /// Implements a motion vector with zoom ratio and spatial rotation
    /// </summary>
    [Serializable]
    public class DepthMotionVector : MotionVectorBase
    {
        /// <summary>
        /// Zoom ratio
        /// </summary>
        public float zoom;
        /// <summary>
        /// Angle of rotation on X axis
        /// </summary>
        public float angleX;
        /// <summary>
        /// Angle of rotation on Y axis
        /// </summary>
        public float angleY;
        /// <summary>
        /// Angle of rotation on Z axis
        /// </summary>
        public float angleZ;

        /// <summary>
        /// Initializer constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="zoom"></param>
        /// <param name="angleX"></param>
        /// <param name="angleY"></param>
        /// <param name="angleZ"></param>
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
}
