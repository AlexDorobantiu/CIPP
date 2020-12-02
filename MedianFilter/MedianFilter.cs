using System;
using System.Collections.Generic;

using ProcessingImageSDK;
using ParametersSDK;

namespace Plugins.Filters.MedianFilter
{
    public class MedianFilter : IFilter
    {
        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>
            {
                new ParametersInt32("Order:", defaultValue: 1, minValue: 1, maxValue: 32, displayType: ParameterDisplayTypeEnum.textBox)
            };
            return parameters;
        }

        private readonly int order;

        public MedianFilter(int order)
        {
            this.order = order;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(order, order, order, order);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark($"Median Filter, order: {order} v1.1, Alex Dorobanțiu");

            int medianSize = (2 * order + 1) * (2 * order + 1);

            if (!inputImage.grayscale)
            {
                byte[,] outputRed = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] outputGreen = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] outputBlue = new byte[inputImage.getSizeY(), inputImage.getSizeX()];

                byte[,] inputRed = inputImage.getRed();
                byte[,] inputGreen = inputImage.getGreen();
                byte[,] inputBlue = inputImage.getBlue();
                
                byte[] medianR = new byte[medianSize];
                byte[] medianG = new byte[medianSize];
                byte[] medianB = new byte[medianSize];

                for (int i = 0; i < outputImage.getSizeY(); i++)
                {
                    for (int j = 0; j < outputImage.getSizeX(); j++)
                    {
                        int elements = 0;
                        for (int k = (i - order > 0 ? i - order : 0); k <= (i + order < inputImage.getSizeY() ? i + order : inputImage.getSizeY() - 1); k++)
                        {
                            for (int l = (j - order > 0 ? j - order : 0); l <= (j + order < inputImage.getSizeX() ? j + order : inputImage.getSizeX() - 1); l++)
                            {
                                medianR[elements] = inputRed[k, l];
                                medianG[elements] = inputGreen[k, l];
                                medianB[elements] = inputBlue[k, l];
                                elements++;
                            }
                        }
                        Array.Sort(medianR, 0, elements);
                        Array.Sort(medianG, 0, elements);
                        Array.Sort(medianB, 0, elements);

                        outputRed[i, j] = medianR[elements / 2];
                        outputGreen[i, j] = medianG[elements / 2];
                        outputBlue[i, j] = medianB[elements / 2];
                    }
                }
                outputImage.setRed(outputRed);
                outputImage.setGreen(outputGreen);
                outputImage.setBlue(outputBlue);
            }
            else
            {
                byte[,] outputGray = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] inputGray = inputImage.getGray();

                byte[] medianGray = new byte[medianSize];
                for (int i = 0; i < outputImage.getSizeY(); i++)
                {
                    for (int j = 0; j < outputImage.getSizeX(); j++)
                    {
                        int elements = 0;
                        for (int k = (i - order > 0 ? i - order : 0); k <= (i + order < inputImage.getSizeY() ? i + order : inputImage.getSizeY() - 1); k++)
                        {
                            for (int l = (j - order > 0 ? j - order : 0); l <= (j + order < inputImage.getSizeX() ? j + order : inputImage.getSizeX() - 1); l++)
                            {
                                medianGray[elements++] = inputGray[k, l];
                            }
                        }
                        Array.Sort(medianGray, 0, elements);
                        outputGray[i, j] = medianGray[elements / 2];
                    }
                }
                outputImage.setGray(outputGray);
            }

            return outputImage;
        }

        #endregion
    }
}
