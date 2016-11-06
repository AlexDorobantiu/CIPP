using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK
{
    [Serializable]
    public struct ImageDependencies
    {
        public int left;
        public int right;
        public int top;
        public int bottom;

        public ImageDependencies(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
    }
}
