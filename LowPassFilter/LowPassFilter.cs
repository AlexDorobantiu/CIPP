using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace Plugins.Filters.LowPassFilter
{
    public class LowPassFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();

        static LowPassFilter()
        {
            parameters.Add(new ParametersInt32(1, 16, 1, "Strength:", DisplayType.textBox));
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int strength;

        public LowPassFilter(int strength)
        {
            this.strength = strength;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(1, 1, 1, 1);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            float[,] f = new float[3, 3];
            f[0, 0] = f[2, 0] = f[0, 2] = f[2, 2] = (float)(1.0 / ((strength + 2) * (strength + 2)));
            f[1, 0] = f[0, 1] = f[2, 1] = f[1, 2] = (float)strength / ((strength + 2) * (strength + 2));
            f[1, 1] = (float)strength * strength / ((strength + 2) * (strength + 2));

            ProcessingImage outputImage = inputImage.mirroredMarginConvolution(f);
            outputImage.addWatermark("Low Pass Filter, strength: " + strength + " v1.0, Alex Dorobantiu");
            return outputImage;
        }

        #endregion
    }
}
