using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK
{
    public static class ProcessingImageUtils
    {
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

        public static int outsideMirroredPosition(int position, int maxPositions)
        {
            if (position < 0)
            {
                position = -position;
            }
            if (position >= maxPositions)
            {
                position = maxPositions + maxPositions - position - 1;
            }
            return position;
        }

        public static byte getPixelMirrored(byte[,] colorChannel, int positionX, int positionY)
        {
            return colorChannel[outsideMirroredPosition(positionY, colorChannel.GetLength(0)), outsideMirroredPosition(positionX, colorChannel.GetLength(1))];
        }

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
    }
}
