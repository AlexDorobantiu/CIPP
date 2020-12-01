using System;
using System.Collections.Generic;

using ProcessingImageSDK;
using ParametersSDK;
using ProcessingImageSDK.Utils;

namespace Plugins.Filters.GaussFilter
{
    public class GaussFilter : IFilter
    {
        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>
            {
                new ParametersInt32(displayName: "Size:", defaultValue: 5, minValue: 3, maxValue: 32, displayType: ParameterDisplayTypeEnum.textBox),
                new ParametersFloat(displayName: "Sigma:", defaultValue: 1, minValue: 0.01f, maxValue: 32, displayType: ParameterDisplayTypeEnum.textBox)
            };
            return parameters;
        }

        private readonly int size;
        private readonly float sigma;

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

            for (int y = -min; y < max; y++)
            {
                for (int x = -min; x < max; x++)
                {
                    gaussConvolutionMatrix[y + min, x + min] = coef1 * (float)Math.Exp(coef2 * (x * x + y * y));
                }
            }

            gaussConvolutionMatrix = ProcessingImageUtils.normalize(gaussConvolutionMatrix);

            ProcessingImage outputImage = inputImage.mirroredMarginConvolution(gaussConvolutionMatrix);
            outputImage.addWatermark($"Gauss Filter, size: {size}, sigma: {sigma} v1.0, Alex Dorobanțiu");
            return outputImage;
        }

        #endregion
    }
}
