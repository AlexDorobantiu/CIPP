namespace ProcessingImageSDK.PixelStructures
{
    /// <summary>
    /// Models a 24bpp pixel by component colors and alpha channel
    /// </summary>
    public struct Pixel32Bpp
    {
        /// <summary>
        /// Component colors and alpha channel
        /// </summary>
        public byte blue, green, red, alpha;
    }
}
