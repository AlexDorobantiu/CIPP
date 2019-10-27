using System.Collections.Generic;

using ProcessingImageSDK;
using ParametersSDK;

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
            return new ImageDependencies(0, 0, 0, 0);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark("Negative Filter, v1.0, Alex Dorobantiu");
            if (!inputImage.grayscale)
            {
                byte[,] red = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] green = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] blue = new byte[inputImage.getSizeY(), inputImage.getSizeX()];

                byte[,] inputRed = inputImage.getRed();
                byte[,] inputGreen = inputImage.getGreen();
                byte[,] inputBlue = inputImage.getBlue();

                for (int i = 0; i < outputImage.getSizeY(); i++)
                {
                    for (int j = 0; j < outputImage.getSizeX(); j++)
                    {
                        red[i, j] = (byte)(255 - inputRed[i, j]);
                        green[i, j] = (byte)(255 - inputGreen[i, j]);
                        blue[i, j] = (byte)(255 - inputBlue[i, j]);
                    }
                }
                outputImage.setRed(red);
                outputImage.setGreen(green);
                outputImage.setBlue(blue);
            }
            else
            {
                byte[,] gray = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] inputGray = inputImage.getGray();
                for (int i = 0; i < outputImage.getSizeY(); i++)
                {
                    for (int j = 0; j < outputImage.getSizeX(); j++)
                    {
                        gray[i, j] = (byte)(255 - inputGray[i, j]);
                    }
                }
                outputImage.setGray(gray);
            }
            return outputImage;
        }

        #endregion
    }
}
