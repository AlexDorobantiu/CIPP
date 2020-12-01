using System.Collections.Generic;

using ProcessingImageSDK;
using ParametersSDK;

namespace Plugins.Masks.ExtremesMask
{
    public class ExtremesMask : IMask
    {
        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>
            {
                new ParametersInt32(displayName: "Upper delta:", defaultValue: 96, minValue: 0, maxValue: 128, displayType: ParameterDisplayTypeEnum.textBox),
                new ParametersInt32(displayName: "Lower delta:", defaultValue: 96, minValue: 0, maxValue: 128, displayType: ParameterDisplayTypeEnum.textBox),
                new ParametersInt32(displayName: "Minimum Area:", defaultValue: 32, minValue: 1, maxValue: int.MaxValue, displayType: ParameterDisplayTypeEnum.textBox)
            };
            return parameters;
        }

        private readonly int upperDelta;
        private readonly int lowerDelta;
        private readonly int minimumAreaSize;

        public ExtremesMask(int upperDelta, int lowerDelta, int minimumAreaSize)
        {
            this.upperDelta = upperDelta;
            this.lowerDelta = lowerDelta;
            this.minimumAreaSize = minimumAreaSize;
        }

        #region IMask Members

        public byte[,] mask(ProcessingImage inputImage)
        {
            int sizeX = inputImage.getSizeX();
            int sizeY = inputImage.getSizeY();
            byte[,] outputMask = new byte[sizeY, sizeX];

            int min = int.MaxValue;
            int max = int.MinValue;

            byte[,] ig = inputImage.getGray();
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (ig[i, j] > max)
                    {
                        max = ig[i, j];
                    }
                    if (ig[i, j] < min)
                    {
                        min = ig[i, j];
                    }
                }
            }

            byte[,] mark = new byte[sizeY, sizeX];
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if ((max - ig[i, j] < upperDelta) || (ig[i, j] - min < lowerDelta))
                    {
                        mark[i, j] = 1;
                    }
                }
            }

            int[] queueX = new int[sizeY * sizeX];
            int[] queueY = new int[sizeY * sizeX];
            int[] deltaX = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] deltaY = { -1, -1, 0, 1, 1, 1, 0, -1 };

            // compute area
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (mark[i, j] == 1)
                    {
                        queueX[0] = j;
                        queueY[0] = i;
                        mark[i, j] = 2;
                        int index = 0;
                        int length = 1;

                        while (index < length)
                        {
                            int positionX = queueX[index];
                            int positionY = queueY[index];

                            for (int deltaIndex = 0; deltaIndex < 8; deltaIndex++)
                            {
                                if ((positionX + deltaX[deltaIndex] >= 0) && (positionY + deltaY[deltaIndex] >= 0) &&
                                    (positionX + deltaX[deltaIndex] < sizeX) && (positionY + deltaY[deltaIndex] < sizeY))
                                {
                                    if (mark[positionY + deltaY[deltaIndex], positionX + deltaX[deltaIndex]] == 1)
                                    {
                                        queueX[length] = positionX + deltaX[deltaIndex];
                                        queueY[length] = positionY + deltaY[deltaIndex];
                                        mark[positionY + deltaY[deltaIndex], positionX + deltaX[deltaIndex]] = 2;
                                        length++;
                                    }
                                }
                            }
                            index++;
                        }

                        if (length > minimumAreaSize)
                        {
                            for (int k = 0; k < length; k++)
                            {
                                outputMask[queueY[k], queueX[k]] = 255;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (outputMask[i, j] == 255)
                    {
                        outputMask[i, j] = 255;
                    }
                    else
                    {
                        outputMask[i, j] = 64;
                    }

                }
            }
            return outputMask;
        }

        #endregion
    }
}
