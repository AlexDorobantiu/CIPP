using System.Collections.Generic;

using ProcessingImageSDK;
using ParametersSDK;

namespace Plugins.Filters.HighPassFilter
{
    public class HighPassFilter : IFilter
    {
        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>
            {
                new ParametersInt32("Strength:", 2, 1, 3, ParameterDisplayTypeEnum.textBox)
            };
            return parameters;
        }

        private readonly int strength;

        public HighPassFilter(int strength)
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
            int[,] f = new int[3, 3];
            switch (strength)
            {
                case 1:
                    {
                        f[1, 0] = f[0, 1] = f[2, 1] = f[1, 2] = -1;
                        f[1, 1] = 5;
                    }
                    break;
                case 2:
                    {
                        f[0, 0] = f[2, 0] = f[0, 2] = f[2, 2] = -1;
                        f[1, 0] = f[0, 1] = f[2, 1] = f[1, 2] = -1;
                        f[1, 1] = 9;
                    }
                    break;
                case 3:
                    {
                        f[0, 0] = f[2, 0] = f[0, 2] = f[2, 2] = 1;
                        f[1, 0] = f[0, 1] = f[2, 1] = f[1, 2] = -2;
                        f[1, 1] = 5;
                    }
                    break;
            }

            ProcessingImage outputImage = inputImage.mirroredMarginConvolution(f);
            outputImage.addWatermark($"High Pass Filter, strength: {strength} v1.0, Alex Dorobanțiu");
            return outputImage;
        }

        #endregion
    }
}
