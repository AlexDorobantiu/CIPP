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
        float sigma;

        public GaussFilter(int size, float sigma)
        {
            this.size = size;
            this.sigma = sigma;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(size - 1, 0, size - 1 , 0);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            float[,] g = new float[size, size];

            float coef1 = (float)(1 / (2 * Math.PI * sigma * sigma));
            float coef2 = -1 / (2 * sigma * sigma);
            int half = size / 2;

            float sum = 0;
            for (int y = -half; y <= half; y++)
                for (int x = -half; x <= half; x++)
                    sum += g[y + half, x + half] = coef1 * (float)Math.Exp(coef2 * (x * x + y * y));
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    g[y, x] /= sum;
            ProcessingImage pi = inputImage.convolution(g);
            pi.addWatermark("Gauss Filter, size: " + size + ", sigma: " + sigma + " v1.0, Alex Dorobantiu");
            return pi;
        }

        #endregion
    }
}
