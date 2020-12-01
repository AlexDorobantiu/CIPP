using System;
using System.Collections.Generic;

using ProcessingImageSDK;
using ParametersSDK;
using ProcessingImageSDK.Utils;

namespace Plugins.Filters.SinFilter
{
    public class SinFilter : IFilter
    {
        private static readonly string[] directionValues = { "horizontal", "vertical" };

        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>
            {
                new ParametersInt32(displayName: "Size:", defaultValue: 5, minValue: 3, maxValue: 32, displayType: ParameterDisplayTypeEnum.textBox),
                new ParametersEnum(displayName: "Direction:", defaultSelected: 0, displayValues: directionValues, displayType: ParameterDisplayTypeEnum.listBox)
            };
            return parameters;
        }

        private readonly int size;
        private readonly int direction;

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
            outputImage.addWatermark($"Sinusoidal Filter, size: {size} direction: {directionValues[direction]} v1.0, Alex Dorobanțiu");
            return outputImage;
        }

        #endregion
    }
}
