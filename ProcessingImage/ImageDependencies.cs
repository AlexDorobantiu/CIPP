using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK
{
    [Serializable]
    public class ImageDependencies
    {
        public int left;
        public int right;
        public int top;
        public int bottom;

        public ImageDependencies()
        {
            this.left = 0;
            this.right = 0;
            this.top = 0;
            this.bottom = 0;
        }

        public ImageDependencies(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
    }
}
