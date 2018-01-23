using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK.MotionVectors
{
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
}
