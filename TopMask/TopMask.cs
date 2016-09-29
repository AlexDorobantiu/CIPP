using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Masks;

namespace Plugins.Filters.TopMask
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

        int upperDelta;
        int minimumAreaSize;

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
            byte[,] newMask = new byte[sizeY, sizeX];

            try
            {
                int max = 255; //int.MinValue;
                byte[,] ig = inputImage.getGray();

                //for (int i = 0; i < sizeY; i++)
                //    for (int j = 0; j < sizeX; j++)
                //        if (ig[i, j] > max) max = ig[i, j];

                byte[,] mark = new byte[sizeY, sizeX];
                for (int i = 0; i < sizeY; i++)
                    for (int j = 0; j < sizeX; j++)
                        if (max - ig[i, j] < upperDelta) mark[i, j] = 1;


                int[] coadaX = new int[sizeY * sizeX];
                int[] coadaY = new int[sizeY * sizeX];
                int[] directionsX = { 0, 1, 1, 1, 0, -1, -1, -1 };
                int[] directionsY = { -1, -1, 0, 1, 1, 1, 0, -1 };

                for (int i = 0; i < sizeY; i++)
                    for (int j = 0; j < sizeX; j++)
                    {
                        if (mark[i, j] == 1)
                        {
                            coadaX[0] = j;
                            coadaY[0] = i;
                            mark[i, j] = 2;
                            int index = 0;
                            int lungime = 1;

                            while (index < lungime)
                            {
                                int pozitieX = coadaX[index];
                                int pozitieY = coadaY[index];

                                for (int d = 0; d < 8; d++)
                                {
                                    if ((pozitieX + directionsX[d] >= 0) && (pozitieY + directionsY[d] >= 0) &&
                                        (pozitieX + directionsX[d] < sizeX) && (pozitieY + directionsY[d] < sizeY))
                                        if (mark[pozitieY + directionsY[d], pozitieX + directionsX[d]] == 1)
                                        {
                                            coadaX[lungime] = pozitieX + directionsX[d];
                                            coadaY[lungime] = pozitieY + directionsY[d];
                                            mark[pozitieY + directionsY[d], pozitieX + directionsX[d]] = 2;
                                            lungime++;
                                        }
                                }
                                index++;
                            }

                            if (lungime > minimumAreaSize)
                            {
                                for (int k = 0; k < lungime; k++)
                                    newMask[coadaY[k], coadaX[k]] = 255;
                            }
                        }
                    }
            }
            catch
            {
            }
            return newMask;
        }

        #endregion
    }
}