using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK.MotionVectors
{
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
}
