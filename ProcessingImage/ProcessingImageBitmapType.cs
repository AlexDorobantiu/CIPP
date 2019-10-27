namespace ProcessingImageSDK
{
    /// <summary>
    /// Describes the output type when converting a ProcessingImage into a Bitmap
    /// </summary>
    public enum ProcessingImageBitmapType
    {
        /// <summary>
        /// Only the alpha channel
        /// </summary>
        Alpha,
        /// <summary>
        /// Alpha channel and all the color channels
        /// </summary>
        AlphaColor,
        /// <summary>
        /// Alpha channel and the red channel
        /// </summary>
        AlphaRed,
        /// <summary>
        /// Alpha channel and the green channel
        /// </summary>
        AlphaGreen,
        /// <summary>
        /// Alpha channel and the blue channel
        /// </summary>
        AlphaBlue,
        /// <summary>
        /// Alpha channel, red channel and the green channel
        /// </summary>
        AlphaRedGreen,
        /// <summary>
        /// Alpha channel, red channel and the blue channel
        /// </summary>
        AlphaRedBlue,
        /// <summary>
        /// Alpha channel, green channel and the blue channel
        /// </summary>
        AlphaGreenBlue,
        /// <summary>
        /// All the color channels
        /// </summary>
        Color,
        /// <summary>
        /// Only the red channel
        /// </summary>
        Red,
        /// <summary>
        /// Only the green channel
        /// </summary>
        Green,
        /// <summary>
        /// Only the blue channel
        /// </summary>
        Blue,
        /// <summary>
        /// The red and the green channel
        /// </summary>
        RedGreen,
        /// <summary>
        /// The red and the blue channel
        /// </summary>
        RedBlue,
        /// <summary>
        /// The green and the blue channel
        /// </summary>
        GreenBlue,
        /// <summary>
        /// The alpha and the gray channel
        /// </summary>
        AlphaGray,
        /// <summary>
        /// Only the gray cannel
        /// </summary>
        Gray,
        /// <summary>
        /// The alpha and the luminance channel
        /// </summary>
        AlphaLuminance,
        /// <summary>
        /// Only the luminance channel
        /// </summary>
        Luminance
    }
}
