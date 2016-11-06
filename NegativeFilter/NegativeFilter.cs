using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace Plugins.Filters.NegativeFilter
{
    public class NegativeFilter : IFilter
    {
        public static List<IParameters> getParametersList()
        {
            return new List<IParameters>();
        }

        public NegativeFilter()
        {
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies();
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark("Negative Filter, v1.0, Alex Dorobantiu");

            if (!inputImage.grayscale)
            {
                byte[,] r = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] g = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] b = new byte[inputImage.getSizeY(), inputImage.getSizeX()];

                byte[,] ir = inputImage.getRed();
                byte[,] ig = inputImage.getGreen();
                byte[,] ib = inputImage.getBlue();

                for (int i = 0; i < outputImage.getSizeY(); i++)
                {
                    for (int j = 0; j < outputImage.getSizeX(); j++)
                    {
                        r[i, j] = (byte)(255 - ir[i, j]);
                        g[i, j] = (byte)(255 - ig[i, j]);
                        b[i, j] = (byte)(255 - ib[i, j]);
                    }
                }
                outputImage.setRed(r);
                outputImage.setGreen(g);
                outputImage.setBlue(b);
            }
            else
            {
                byte[,] gray = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] ig = inputImage.getGray();
                for (int i = 0; i < outputImage.getSizeY(); i++)
                {
                    for (int j = 0; j < outputImage.getSizeX(); j++)
                    {
                        gray[i, j] = (byte)(255 - ig[i, j]);
                    }
                }
                outputImage.setGray(gray);
            }

            return outputImage;
        }

        #endregion
    }
}
