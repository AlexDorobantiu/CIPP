using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace Plugins.Filters.LaplacianOfGaussian
{
    public class LaplacianOfGaussian : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();

        static LaplacianOfGaussian()
        {
            parameters.Add(new ParametersInt32(1, 16, 1, "Strength:", DisplayType.textBox));
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int strength;

        public LaplacianOfGaussian(int strength)
        {
            this.strength = strength;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(2, 2, 2, 2);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            int[,] f = new int[5, 5];
            f[0, 0] = f[0, 1] = f[0, 3] = f[0, 4] = 0;
            f[0, 2] = -1;
            f[1, 0] = f[1, 4] = 0;
            f[1, 1] = f[1, 3] = -1;
            f[1, 2] = -2;
            f[2, 0] = f[2, 4] = -1;
            f[2, 1] = f[2, 3] = -2;
            f[2, 2] = 16;
            f[3, 0] = f[3, 4] = 0;
            f[3, 1] = f[3, 3] = -1;
            f[3, 2] = -2;
            f[4, 0] = f[4, 1] = f[4, 3] = f[4, 4] = 0;
            f[4, 2] = -1;
            ProcessingImage outputImage = inputImage.mirroredMarginConvolution(f);
            outputImage.addWatermark("Low Pass Filter, strength: " + strength + " v1.0, Alex Dorobantiu");
            return outputImage;
        }

        #endregion
    }
}
