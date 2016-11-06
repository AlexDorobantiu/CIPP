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
        private static string[] directionValues = { "horizontal", "vertical" };

        static SinFilter()
        {
            parameters.Add(new ParametersInt32(3, 32, 5, "Size:", DisplayType.textBox));           
            
            parameters.Add(new ParametersEnum("Direction:", 0, directionValues, DisplayType.listBox));
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int size;
        private int direction;

        public SinFilter(int size, int direction)
        {
            this.size = size;
            this.direction = direction;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(-1, -1, -1, -1);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            float[,] sinFilterMatrix = new float[size, size];
            float step = (float)(2 * Math.PI / size);

            float position = 0;
            if (direction == 0)
            {
                for (int x = 0; x < size; x++)
                {
                    sinFilterMatrix[0, x] = (float)(-Math.Sin(position));
                    position += step;
                }
                for (int y = size - 1; y >= 1; y--)
                {
                    for (int x = 0; x < size; x++)
                    {
                        sinFilterMatrix[y, x] = sinFilterMatrix[0, x];
                    }
                }
            }
            else
            {
                for (int y = 0; y < size; y++)
                {
                    sinFilterMatrix[y, 0] = (float)(-Math.Sin(position));
                    position += step;
                }

                for (int x = size - 1; x >= 0; x--)
                {
                    for (int y = 0; y < size; y++)
                    {
                        sinFilterMatrix[y, x] = sinFilterMatrix[y, 0];
                    }
                }
            }

            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);

            float[,] filteredImage = ProcessingImageUtils.mirroredMarginConvolution(inputImage.getGray(), sinFilterMatrix);
            byte[,] gray = ProcessingImageUtils.fitHistogramToDisplay(filteredImage);

            outputImage.setGray(gray);
            outputImage.addWatermark("Sinusoidal Filter, size: " + size + " direction: " + directionValues[direction] + " v1.0, Alex Dorobantiu");
            return outputImage;
        }

        #endregion
    }
}
