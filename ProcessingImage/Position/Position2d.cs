using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK.Position
{
    /// <summary>
    /// Useful for describing pixel positions inside an image
    /// </summary>
    [Serializable]
    public struct Position2d
    {
        public int x;
        public int y;

        public Position2d(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
