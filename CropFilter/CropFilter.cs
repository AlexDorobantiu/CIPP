using System;
using System.Collections.Generic;
using System.Text;
using ParametersSDK;
using Plugins.Filters;
using ProcessingImageSDK;

namespace Plugins.Filters.CropFilter
{
    public class CropFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();

        static CropFilter()
        {
            parameters.Add(new ParametersInt32(0, int.MaxValue, 0, "Left:", DisplayType.textBox));
            parameters.Add(new ParametersInt32(0, int.MaxValue, 0, "Rigth:", DisplayType.textBox));
            parameters.Add(new ParametersInt32(0, int.MaxValue, 0, "Top:", DisplayType.textBox));
            parameters.Add(new ParametersInt32(0, int.MaxValue, 0, "Bottom:", DisplayType.textBox));
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int left, right, top, bottom;

        public CropFilter(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(-1, -1, -1, -1);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            int outputSizeX = inputImage.getSizeX() - left - right;
            int outputSizeY = inputImage.getSizeY() - top - bottom;

            if (outputSizeX <= 0 || outputSizeY <= 0)
            {
                return inputImage;
            }

            ProcessingImage outputImage = new ProcessingImage();
            outputImage.initialize(inputImage.getName(), outputSizeX, outputSizeY, false);

            byte[,] inputAlpha = inputImage.getAlpha();
            byte[,] outputAlpha = new byte[outputSizeY, outputSizeX];

            if (inputImage.grayscale)
            {
                byte[,] inputGray = inputImage.getGray();
                byte[,] outputGray = new byte[outputSizeY, outputSizeX];

                for (int i = 0; i < outputSizeY; i++)
                {
                    for (int j = 0; j < outputSizeX; j++)
                    {
                        outputAlpha[i, j] = inputAlpha[i + top, j + left];
                        outputGray[i, j] = inputGray[i + top, j + left];
                    }
                }

                outputImage.setGray(outputGray);
            }
            else
            {
                byte[,] outputRed = new byte[outputSizeY, outputSizeX];
                byte[,] outputGreen = new byte[outputSizeY, outputSizeX];
                byte[,] outputBlue = new byte[outputSizeY, outputSizeX];

                for (int i = 0; i < outputSizeY; i++)
                {
                    for (int j = 0; j < outputSizeX; j++)
                    {
                        outputAlpha[i, j] = inputAlpha[i + top, j + left];
                        outputRed[i, j] = inputImage.getRed()[i + top, j + left];
                        outputGreen[i, j] = inputImage.getGreen()[i + top, j + left];
                        outputBlue[i, j] = inputImage.getBlue()[i + top, j + left];
                    }
                }

                outputImage.setRed(outputRed);
                outputImage.setGreen(outputGreen);
                outputImage.setBlue(outputBlue);
            }
            outputImage.setAlpha(outputAlpha);

            outputImage.addWatermark("Crop filter Left: " + left + " Right: " + right + " Top: " + top + " Bottom: " + bottom + " v1.0, Alex Dorobantiu");
            return outputImage;
        }
    }
}
