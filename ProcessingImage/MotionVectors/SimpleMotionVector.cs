using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK.MotionVectors
{
    [Serializable]
    public class SimpleMotionVector : MotionVectorBase
    {
        public SimpleMotionVector(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
