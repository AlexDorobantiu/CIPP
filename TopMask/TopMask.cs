using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Masks;

namespace Plugins.Masks.TopMask
{
    public class TopMask : IMask
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();

        static TopMask()
        {
            parameters.Add(new ParametersInt32(0, 255, 128, "Upper delta:", DisplayType.textBox));
            parameters.Add(new ParametersInt32(1, int.MaxValue, 32, "Minimum Area:", DisplayType.textBox));
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int upperDelta;
        private int minimumAreaSize;

        public TopMask(int upperDelta, int minimumAreaSize)
        {
            this.upperDelta = upperDelta;
            this.minimumAreaSize = minimumAreaSize;
        }

        #region IMask Members

        public byte[,] mask(ProcessingImage inputImage)
        {
            int sizeX = inputImage.getSizeX();
            int sizeY = inputImage.getSizeY();

            int max = 255;
            byte[,] inputGray = inputImage.getGray();

            byte[,] mark = new byte[sizeY, sizeX];
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (max - inputGray[i, j] < upperDelta)
                    {
                        mark[i, j] = 1;
                    }
                }
            }

            int[] queueX = new int[sizeY * sizeX];
            int[] queueY = new int[sizeY * sizeX];
            int[] deltaX = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] deltaY = { -1, -1, 0, 1, 1, 1, 0, -1 };

            byte[,] newMask = new byte[sizeY, sizeX];

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
                                newMask[queueY[k], queueX[k]] = 255;
                            }
                        }
                    }
                }
            }
            return newMask;
        }

        #endregion
    }
}