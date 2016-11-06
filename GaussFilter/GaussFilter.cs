using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace Plugins.Filters.GaussFilter
{
    public class GaussFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();

        static GaussFilter()
        {
            parameters.Add(new ParametersInt32(3, 32, 5, "Size:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(0.01f, 32, 1, "Sigma:", DisplayType.textBox));
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int size;
        private float sigma;

        public GaussFilter(int size, float sigma)
        {
            this.size = size;
            this.sigma = sigma;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(size / 2, size / 2 - size % 2, size / 2, size / 2 - size % 2);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            float[,] gaussConvolutionMatrix = new float[size, size];

            float coef1 = (float)(1 / (2 * Math.PI * sigma * sigma));
            float coef2 = -1 / (2 * sigma * sigma);
            int min = size / 2;
            int max = size / 2 + size % 2;

            float sum = 0;
            for (int y = -min; y < max; y++)
            {
                for (int x = -min; x < max; x++)
                {
                    sum += gaussConvolutionMatrix[y + min, x + min] = coef1 * (float)Math.Exp(coef2 * (x * x + y * y));
                }
            }

            // normalize
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    gaussConvolutionMatrix[y, x] /= sum;
                }
            }

            ProcessingImage outputImage = inputImage.mirroredMarginConvolution(gaussConvolutionMatrix);
            outputImage.addWatermark("Gauss Filter, size: " + size + ", sigma: " + sigma + " v1.0, Alex Dorobantiu");
            return outputImage;
        }

        #endregion
    }
}
