using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK
{
    [Serializable]
    public class ImageDependencies
    {
        public readonly int left;
        public readonly int right;
        public readonly int top;
        public readonly int bottom;

        public ImageDependencies(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
    }
}
