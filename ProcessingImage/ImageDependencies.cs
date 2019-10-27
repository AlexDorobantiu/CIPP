using System;

namespace ProcessingImageSDK
{
    /// <summary>
    /// Describes the dependency distance of a filter. Used for splitting an image for parallel processing
    /// </summary>
    [Serializable]
    public class ImageDependencies
    {
        /// <summary>
        /// Dependency on the left
        /// </summary>
        public readonly int left;
        /// <summary>
        /// Dependency on the right
        /// </summary>
        public readonly int right;
        /// <summary>
        /// Dependency on the top
        /// </summary>
        public readonly int top;
        /// <summary>
        /// Dependency on the bottom
        /// </summary>
        public readonly int bottom;

        /// <summary>
        /// Initializer constructor
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        public ImageDependencies(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
    }
}
