using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace Plugins.Filters.SinFilter
{
    public class SinFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();
        static SinFilter()
        {
            parameters.Add(new ParametersInt32(3, 32, 5, "Size:", DisplayType.textBox));           
            string[] values = { "horizontal", "vertical" };
            parameters.Add(new ParametersEnum("Direction:", 0, values, DisplayType.listBox));
            parameters.Add(new ParametersInt32(0, 512, 128, "Threshold:", DisplayType.textBox));
        }
        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int size;
        private int direction;
        private int threshold;

        public SinFilter(int size, int direction, int threshold)
        {
            this.size = size;
            this.direction = direction;
            this.threshold = threshold;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(size - 1, 0, size - 1, 0);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            float[,] matrix = new float[size, size];
            //float[] sumArray = new float[size];

            float step = 6.2432f / (size - 1); //2 * pi

            float s = 0;
            float position = 0;

            if (direction == 0)
            {
                for (int x = 0; x < size; x++)
                {
                    matrix[0, x] = (float)(-Math.Sin(position));
                    position += step;
                    s += matrix[0, x];
                }

                //s *= size;

                //if (s != 0)
                //    for (int y = size - 1; y >= 0; y--)
                //    {
                //        for (int x = 0; x < size; x++)
                //        {
                //            matrix[y, x] = matrix[0, x] / s;
                //        }
                //    }
                for (int y = size - 1; y >= 1; y--)
                {
                    for (int x = 0; x < size; x++)
                    {
                        matrix[y, x] = matrix[0, x];
                    }
                }
            }
            else
            {
                for (int y = 0; y < size; y++)
                {
                    matrix[y, 0] = (float)(-Math.Sin(position));
                    position += step;
                    s += matrix[y, 0];
                }

                s *= size;

                if (s != 0)
                    for (int x = size - 1; x >= 0; x--)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            matrix[y, x] = matrix[y, 0] / s;
                        }
                    }
            }

            //ProcessingImage pi = inputImage.convolution(g);
            //pi.addWatermark("Gauss Filter, size: " + size + " v1.0, Alex Dorobantiu");
            //return pi;

            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributesAndAlpha(inputImage);

            inputImage.setSizeX(pi.getSizeX() - size + 1);
            inputImage.setSizeY(pi.getSizeY() - size + 1);
            int imageYSize = pi.getSizeY();
            int imageXSize = pi.getSizeX();

            int[,] filteredImage = new int[imageYSize, imageXSize];
            byte[,] g = new byte[imageYSize, imageXSize];
            byte[,] ig = inputImage.getGray();

            int max = int.MinValue;
            int min = int.MaxValue;

            for (int i = 0; i < imageYSize; i++)
            {
                for (int j = 0; j < imageXSize; j++)
                {
                    float sum = 0;
                    for (int k = size - 1; k >= 0; k--)
                        for (int l = size - 1; l >= 0; l--)
                            sum += (float)(ig[i + size - 1 - k, j + size - 1 - l] * matrix[k, l]);
                    //for (int k = 0; k < size; k++)
                    //{
                        
                    //    for (int l = 0; l < size; l++)
                    //    {
                    //        sumArray[l] = (ig[i - k, j - l] * matrix[k, l]);
                    //    }

                    //    // true == plus
                    //    bool sign = sumArray[0] > 0; 
                    //    int length = 1;

                    //    for (int l = 1; l < size; l++)
                    //    {
                    //        if (sign && sumArray[l] > 0)
                    //        {
                    //            if (sumArray[l] > threshold)
                    //            {
                    //                length++;
                    //            }
                    //            else
                    //            {
                    //                for (x = 0; x < length; x++) ;
                    //                   // sumArray[
                    //            }
                    //        }
                    //        else {
                    //            if (!sign && sumArray[l] < 0) ;
                    //            else
                    //            {
                    //            }
                    //        }
                    //    }

                    //    for (int l = 0; l < size; l++)
                    //    {
                    //        sum += sumArray[l];
                    //    }
                    //}
                    //sum = Math.Abs(sum);
                    if (sum > max) max = (int)sum;
                    if (sum < min) min = (int)sum;
                    filteredImage[i, j] = (int)sum;
                }
            }

            for (int i = 0; i < imageYSize; i++)
                for (int j = 0; j < imageXSize; j++)
                    g[i, j] = (byte)(((filteredImage[i, j] - min) * 255) / (max - min));

            pi.setGray(g);

            return pi;
        }

        #endregion
    }
}
