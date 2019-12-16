namespace ProcessingImageSDK
{
    /// <summary>
    /// Utility class for working with image processing
    /// </summary>
    public static class ProcessingImageUtils
    {
        /// <summary>
        /// Computes the result of the convolution with the floating point kernel specified
        /// </summary>
        /// <param name="colorChannel"></param>
        /// <param name="convolutionMatrix"></param>
        /// <returns></returns>
        public static float[,] delayedConvolution(byte[,] colorChannel, float[,] convolutionMatrix)
        {
            int colorChannelSizeX = colorChannel.GetLength(1);
            int colorChannelSizeY = colorChannel.GetLength(0);
            int filterSizeX = convolutionMatrix.GetLength(1);
            int filterSizeY = convolutionMatrix.GetLength(0);
            int delayX = filterSizeX - 1;
            int delayY = filterSizeY - 1;

            float[,] output = new float[colorChannelSizeY, colorChannelSizeX];
            for (int y = 0; y < colorChannelSizeY - filterSizeY + 1; y++)
            {
                for (int x = 0; x < colorChannelSizeX - filterSizeX + 1; x++)
                {
                    float sum = 0;
                    for (int i = 0; i < filterSizeY; i++)
                    {
                        for (int j = 0; j < filterSizeX; j++)
                        {
                            sum += convolutionMatrix[i, j] * colorChannel[y + i, x + j];
                        }
                    }
                    output[y + delayY, x + delayX] = sum;
                }
            }
            return output;
        }

        /// <summary>
        /// Computes the result of the convolution with the integer kernel specified
        /// </summary>
        /// <param name="colorChannel"></param>
        /// <param name="convolutionMatrix"></param>
        /// <returns></returns>
        public static int[,] delayedConvolution(byte[,] colorChannel, int[,] convolutionMatrix)
        {
            int colorChannelSizeX = colorChannel.GetLength(1);
            int colorChannelSizeY = colorChannel.GetLength(0);
            int filterSizeX = convolutionMatrix.GetLength(1);
            int filterSizeY = convolutionMatrix.GetLength(0);
            int delayX = filterSizeX - 1;
            int delayY = filterSizeY - 1;

            int[,] output = new int[colorChannelSizeY, colorChannelSizeX];
            for (int y = 0; y < colorChannelSizeY - filterSizeY + 1; y++)
            {
                for (int x = 0; x < colorChannelSizeX - filterSizeX + 1; x++)
                {
                    int sum = 0;
                    for (int i = 0; i < filterSizeY; i++)
                    {
                        for (int j = 0; j < filterSizeX; j++)
                        {
                            sum += convolutionMatrix[i, j] * colorChannel[y + i, x + j];
                        }
                    }
                    output[y + delayY, x + delayX] = sum;
                }
            }
            return output;
        }

        /// <summary>
        /// Gets a position in a mirrored way when outside of the interval [0, maxPosition)
        /// </summary>
        /// <param name="position"></param>
        /// <param name="maxPosition"></param>
        /// <returns></returns>
        public static int outsideMirroredPosition(int position, int maxPosition)
        {
            if (position < 0)
            {
                position = -position;
            }
            if (position >= maxPosition)
            {
                position = maxPosition + maxPosition - position - 1;
            }
            return position;
        }

        /// <summary>
        /// Gets a pixel of a color channel at the position received as parameter. When the position is outside of the matrix, the mirrored position is used.
        /// </summary>
        /// <param name="colorChannel"></param>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <returns></returns>
        public static byte getPixelMirrored(byte[,] colorChannel, int positionX, int positionY)
        {
            return colorChannel[outsideMirroredPosition(positionY, colorChannel.GetLength(0)), outsideMirroredPosition(positionX, colorChannel.GetLength(1))];
        }

        /// <summary>
        /// Computes the result of the convolution with the floating point kernel specified and keeping the same output size as the input size by mirroring the margins of the channel.
        /// </summary>
        /// <param name="colorChannel"></param>
        /// <param name="convolutionMatrix"></param>
        /// <returns></returns>
        public static float[,] mirroredMarginConvolution(byte[,] colorChannel, float[,] convolutionMatrix)
        {
            int colorChannelSizeX = colorChannel.GetLength(1);
            int colorChannelSizeY = colorChannel.GetLength(0);
            int filterSizeX = convolutionMatrix.GetLength(1);
            int filterSizeY = convolutionMatrix.GetLength(0);
            int filterMinX = filterSizeX / 2;
            int filterMinY = filterSizeY / 2;
            int filterMaxX = filterSizeX / 2 + filterSizeX % 2;
            int filterMaxY = filterSizeY / 2 + filterSizeY % 2;

            float[,] output = new float[colorChannelSizeY, colorChannelSizeX];

            for (int y = 0; y < colorChannelSizeY; y++)
            {
                for (int x = 0; x < colorChannelSizeX; x++)
                {
                    float sum = 0;
                    for (int i = -filterMinY; i < filterMaxY; i++)
                    {
                        for (int j = -filterMinX; j < filterMaxX; j++)
                        {
                            sum += convolutionMatrix[i + filterMinY, j + filterMinX] *
                                colorChannel[outsideMirroredPosition(y + i, colorChannelSizeY), outsideMirroredPosition(x + j, colorChannelSizeX)];
                        }
                    }
                    output[y, x] = sum;
                }
            }
            return output;
        }

        /// <summary>
        /// Computes the result of the convolution with the floating point kernel specified and keeping the same output size as the input size by mirroring the margins of the input.
        /// </summary>
        /// <param name="inputMatrix"></param>
        /// <param name="convolutionMatrix"></param>
        /// <returns></returns>
        public static float[,] mirroredMarginConvolution(float[,] inputMatrix, float[,] convolutionMatrix)
        {
            int sizeX = inputMatrix.GetLength(1);
            int sizeY = inputMatrix.GetLength(0);
            int filterSizeX = convolutionMatrix.GetLength(1);
            int filterSizeY = convolutionMatrix.GetLength(0);
            int filterMinX = filterSizeX / 2;
            int filterMinY = filterSizeY / 2;
            int filterMaxX = filterSizeX / 2 + filterSizeX % 2;
            int filterMaxY = filterSizeY / 2 + filterSizeY % 2;

            float[,] output = new float[sizeY, sizeX];

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    float sum = 0;
                    for (int i = -filterMinY; i < filterMaxY; i++)
                    {
                        for (int j = -filterMinX; j < filterMaxX; j++)
                        {
                            sum += convolutionMatrix[i + filterMinY, j + filterMinX] *
                                inputMatrix[outsideMirroredPosition(y + i, sizeY), outsideMirroredPosition(x + j, sizeX)];
                        }
                    }
                    output[y, x] = sum;
                }
            }
            return output;
        }

        /// <summary>
        /// Computes the result of the convolution with the integer kernel specified and keeping the same output size as the input size by mirroring the margins of the channel.
        /// </summary>
        /// <param name="colorChannel"></param>
        /// <param name="convolutionMatrix"></param>
        /// <returns></returns>
        public static int[,] mirroredMarginConvolution(byte[,] colorChannel, int[,] convolutionMatrix)
        {
            int colorChannelSizeX = colorChannel.GetLength(1);
            int colorChannelSizeY = colorChannel.GetLength(0);
            int filterSizeX = convolutionMatrix.GetLength(1);
            int filterSizeY = convolutionMatrix.GetLength(0);
            int filterMinX = filterSizeX / 2;
            int filterMinY = filterSizeY / 2;
            int filterMaxX = filterSizeX / 2 + filterSizeX % 2;
            int filterMaxY = filterSizeY / 2 + filterSizeY % 2;

            int[,] output = new int[colorChannelSizeY, colorChannelSizeX];

            for (int y = 0; y < colorChannelSizeY; y++)
            {
                for (int x = 0; x < colorChannelSizeX; x++)
                {
                    int sum = 0;
                    for (int i = -filterMinY; i < filterMaxY; i++)
                    {
                        for (int j = -filterMinX; j < filterMaxX; j++)
                        {
                            sum += convolutionMatrix[i + filterMinY, j + filterMinX] *
                                colorChannel[outsideMirroredPosition(y + i, colorChannelSizeY), outsideMirroredPosition(x + j, colorChannelSizeX)];
                        }
                    }
                    output[y, x] = sum;
                }
            }
            return output;
        }

        /// <summary>
        /// Computes the result of the convolution with the integer kernel specified and keeping the same output size as the input size by mirroring the margins of the input.
        /// </summary>
        /// <param name="inputMatrix"></param>
        /// <param name="convolutionMatrix"></param>
        /// <returns></returns>
        public static int[,] mirroredMarginConvolution(int[,] inputMatrix, int[,] convolutionMatrix)
        {
            int sizeX = inputMatrix.GetLength(1);
            int sizeY = inputMatrix.GetLength(0);
            int filterSizeX = convolutionMatrix.GetLength(1);
            int filterSizeY = convolutionMatrix.GetLength(0);
            int filterMinX = filterSizeX / 2;
            int filterMinY = filterSizeY / 2;
            int filterMaxX = filterSizeX / 2 + filterSizeX % 2;
            int filterMaxY = filterSizeY / 2 + filterSizeY % 2;

            int[,] output = new int[sizeY, sizeX];

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    int sum = 0;
                    for (int i = -filterMinY; i < filterMaxY; i++)
                    {
                        for (int j = -filterMinX; j < filterMaxX; j++)
                        {
                            sum += convolutionMatrix[i + filterMinY, j + filterMinX] *
                                inputMatrix[outsideMirroredPosition(y + i, sizeY), outsideMirroredPosition(x + j, sizeX)];
                        }
                    }
                    output[y, x] = sum;
                }
            }
            return output;
        }

        /// <summary>
        /// Does a proportional input fit into the [0, 255] interval
        /// </summary>
        /// <param name="inputMatrix"></param>
        /// <returns></returns>
        public static byte[,] fitHistogramToDisplay(int[,] inputMatrix)
        {
            int sizeX = inputMatrix.GetLength(1);
            int sizeY = inputMatrix.GetLength(0);
            byte[,] result = new byte[sizeY, sizeX];

            int max = int.MinValue;
            int min = int.MaxValue;
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (inputMatrix[i, j] > max)
                    {
                        max = inputMatrix[i, j];
                    }
                    if (inputMatrix[i, j] < min)
                    {
                        min = inputMatrix[i, j];
                    }
                }
            }

            if (max != min)
            {
                for (int i = 0; i < sizeY; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        result[i, j] = (byte)(((inputMatrix[i, j] - min) * 255.0f) / (max - min) + 0.5f);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Does a proportional input fit into the [0, 255] interval
        /// </summary>
        /// <param name="inputMatrix"></param>
        /// <returns></returns>
        public static byte[,] fitHistogramToDisplay(float[,] inputMatrix)
        {
            int sizeX = inputMatrix.GetLength(1);
            int sizeY = inputMatrix.GetLength(0);
            byte[,] result = new byte[sizeY, sizeX];

            float max = int.MinValue;
            float min = int.MaxValue;
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (inputMatrix[i, j] > max)
                    {
                        max = inputMatrix[i, j];
                    }
                    if (inputMatrix[i, j] < min)
                    {
                        min = inputMatrix[i, j];
                    }
                }
            }

            if (max != min)
            {
                for (int i = 0; i < sizeY; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        result[i, j] = (byte)(((inputMatrix[i, j] - min) * 255.0f) / (max - min) + 0.5f);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Does a input truncate into the [0, 255] interval
        /// </summary>
        /// <param name="inputMatrix"></param>
        /// <returns></returns>
        public static byte[,] truncateToDisplay(int[,] inputMatrix)
        {
            int sizeX = inputMatrix.GetLength(1);
            int sizeY = inputMatrix.GetLength(0);
            byte[,] result = new byte[sizeY, sizeX];

            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    int value = inputMatrix[i, j];
                    if (value < 0)
                    {
                        value = 0;
                    }
                    if (value > 255)
                    {
                        value = 255;
                    }
                    result[i, j] = (byte)value;
                }
            }
            return result;
        }

        /// <summary>
        /// Does a input truncate into the [0, 255] interval
        /// </summary>
        /// <param name="inputMatrix"></param>
        /// <returns></returns>
        public static byte[,] truncateToDisplay(float[,] inputMatrix)
        {
            int sizeX = inputMatrix.GetLength(1);
            int sizeY = inputMatrix.GetLength(0);
            byte[,] result = new byte[sizeY, sizeX];

            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    float value = inputMatrix[i, j];
                    if (value < 0)
                    {
                        value = 0;
                    }
                    if (value > 255)
                    {
                        value = 255;
                    }
                    result[i, j] = (byte)(value + 0.5f);
                }
            }
            return result;
        }

        /// <summary>
        /// Converts a color channel to a floating poing matrix
        /// </summary>
        /// <param name="colorChannel"></param>
        /// <returns></returns>
        public static float[,] convertToFloat(byte[,] colorChannel)
        {
            int sizeX = colorChannel.GetLength(1);
            int sizeY = colorChannel.GetLength(0);
            float[,] result = new float[sizeY, sizeX];

            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    result[i, j] = colorChannel[i, j];
                }
            }

            return result;
        }

        /// <summary>
        /// Normalizes the input matrix
        /// </summary>
        /// <param name="matrix"></param>
        public static float[,] normalize(float[,] matrix)
        {
            int sizeX = matrix.GetLength(1);
            int sizeY = matrix.GetLength(0);
            float[,] result = new float[sizeY, sizeX];

            float sum = 0;
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    sum += matrix[i, j];
                }
            }
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    result[i, j] = matrix[i, j] / sum;
                }
            }
            return result;
        }

        /// <summary>
        /// Normalizes the input matrix
        /// </summary>
        /// <param name="matrix"></param>
        public static double[,] normalize(double[,] matrix)
        {
            int sizeX = matrix.GetLength(1);
            int sizeY = matrix.GetLength(0);
            double[,] result = new double[sizeY, sizeX];

            double sum = 0;
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    sum += matrix[i, j];
                }
            }
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    result[i, j] = matrix[i, j] / sum;
                }
            }
            return result;
        }

        /// <summary>
        /// Divides the values in the matrix to the sum of the positive values,
        /// thus limiting the maximum value of a convolution operation to 1
        /// </summary>
        public static float[,] semiNormalize(float[,] matrix)
        {
            int sizeX = matrix.GetLength(1);
            int sizeY = matrix.GetLength(0);
            float[,] result = new float[sizeY, sizeX];

            float sum = 0;
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (matrix[i, j] > 0)
                    {
                        sum += matrix[i, j];
                    }
                }
            }
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    result[i, j] = matrix[i, j] / sum;
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a new color channel with the specified dimension and a default color
        /// </summary>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        /// <param name="defaultColor"></param>
        /// <returns></returns>
        public static byte[,] createChannel(int sizeX, int sizeY, byte defaultColor = 0)
        {
            byte[,] channel = new byte[sizeY, sizeX];
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    channel[i, j] = defaultColor;
                }
            }
            return channel;
        }
    }
}
