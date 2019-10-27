using System;

namespace ProcessingImageSDK.Position
{
    /// <summary>
    /// Useful for describing pixel positions inside an image
    /// </summary>
    [Serializable]
    public struct Position2d
    {
        /// <summary>
        /// Position on the horizontal axis
        /// </summary>
        public int x;
        /// <summary>
        /// Position on the vertical axis
        /// </summary>
        public int y;

        /// <summary>
        /// Initializer constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Position2d(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
