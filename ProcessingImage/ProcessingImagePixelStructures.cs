using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK
{
    public struct Pixel32Bpp
    {
        public byte blue, green, red, alpha;
    }

    public struct Pixel24Bpp
    {
        public byte blue, green, red;
    }

    public struct Pixel8Bpp
    {
        public byte gray;
    }
}
