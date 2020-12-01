using System.Collections.Generic;

using ProcessingImageSDK;
using ParametersSDK;

namespace Plugins.Filters.LowPassFilter
{
    public class LowPassFilter : IFilter
    {
        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>
            {
                new ParametersInt32(displayName: "Strength:", defaultValue: 1, minValue: 1, maxValue: 16, displayType: ParameterDisplayTypeEnum.textBox)
            };
            return parameters;
        }

        private readonly int strength;

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
            outputImage.addWatermark($"Low Pass Filter, strength: {strength} v1.0, Alex Dorobanțiu");
            return outputImage;
        }

        #endregion
    }
}
