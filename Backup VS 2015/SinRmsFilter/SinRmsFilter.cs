using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace SinRmsFilter
{
    public class SinRmsFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();
        static SinRmsFilter()
        {
            parameters.Add(new ParametersInt32(3, 32, 5, "Size:", DisplayType.textBox));           
            string[] values = { "horizontal", "vertical" };
            parameters.Add(new ParametersEnum("Direction:", 0, values, DisplayType.listBox));
            parameters.Add(new ParametersInt32(0, 512, 50, "Threshold:", DisplayType.textBox));
        }
        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int size;
        private int direction;
        private int threshold;

        public SinRmsFilter(int size, int direction, int threshold)
        {
            this.size = size;
            this.direction = direction;
            this.threshold = threshold;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(size - 1, 0, 0, 0);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            //prepare filter
            float[] sine = new float[size];
            float position = 0;
            float step = 6.2432f / (size - 1); //2 * pi
            for (int x = 0; x < size; x++)
            {
                sine[x] = -(float)Math.Sin(position);
                position += step;
            }

            //prepare image
            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributesAndAlpha(inputImage);

            int imageYSize = pi.getSizeY();
            int imageXSize = pi.getSizeX();

            byte[,] g = new byte[imageYSize, imageXSize];
            byte[,] ig = inputImage.getGray();
            int size2 = size / 2;

            for (int i = 0; i < imageYSize; i++)
            {
                for (int j = size - 1; j < imageXSize; j++)
                {
                    float sum1 = 0;
                    float sum2 = 0;

                    for (int k = size2 - 1; k >= 0; k--)
                        sum1 += (float)((ig[i, j - k] - 128) * sine[k]);
                    for (int k = size - 1; k >= size2; k--)
                        sum2 += (float)((ig[i, j - k] - 128) * sine[k]);

                    if (sum1 >= threshold && sum2 >= threshold)
                    {
                        g[i, j] = 255;
                    }
                }
            }

            pi.setGray(g);

            return pi;
        }

        #endregion
    }
    
}
