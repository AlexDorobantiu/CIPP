﻿using System;
using System.Collections.Generic;

using ProcessingImageSDK;
using ParametersSDK;

namespace Plugins.Filters.MedianKeepFilter
{
    public class MedianKeepFilter : IFilter
    {
        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>
            {
                new ParametersInt32(displayName: "Order:", defaultValue: 1, minValue: 1, maxValue: 32, displayType: ParameterDisplayTypeEnum.textBox)
            };
            return parameters;
        }

        private readonly int order;

        public MedianKeepFilter(int order)
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
            outputImage.addWatermark($"Median Keep Filter, order: {order} v1.0, Alex Dorobanțiu");

            int medianSize = (2 * order + 1) * (2 * order + 1);
            int medianPosition = medianSize / 2;

            if (!inputImage.grayscale)
            {
                byte[,] outputRed = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] outputGreen = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] outputBlue = new byte[inputImage.getSizeY(), inputImage.getSizeX()];

                byte[,] inputRed = inputImage.getRed();
                byte[,] inputGreen = inputImage.getGreen();
                byte[,] inputBlue = inputImage.getBlue();
                byte[,] inputLuminance = inputImage.getLuminance();
                
                byte[] medianLuminance = new byte[medianSize];

                for (int i = order; i < outputImage.getSizeY() - order; i++)
                {
                    for (int j = order; j < outputImage.getSizeX() - order; j++)
                    {
                        int pivot = 0;
                        for (int k = i - order; k <= i + order; k++)
                        {
                            for (int l = j - order; l <= j + order; l++)
                            {
                                medianLuminance[pivot++] = inputLuminance[k, l];
                            }
                        }
                        Array.Sort(medianLuminance);
                        byte y = medianLuminance[medianPosition];
                        for (int k = i - order; k <= i + order; k++)
                        {
                            for (int l = j - order; l <= j + order; l++)
                            {
                                if (inputLuminance[k, l] == y)
                                {
                                    outputRed[i, j] = inputRed[k, l];
                                    outputGreen[i, j] = inputGreen[k, l];
                                    outputBlue[i, j] = inputBlue[k, l];
                                    break;
                                }
                            }
                        }
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
                for (int i = order; i < outputImage.getSizeY() - order; i++)
                {
                    for (int j = order; j < outputImage.getSizeX() - order; j++)
                    {
                        int pivot = 0;
                        for (int k = i - order; k <= i + order; k++)
                        {
                            for (int l = j - order; l <= j + order; l++)
                            {
                                medianGray[pivot++] = inputGray[k, l];
                            }
                        }
                        Array.Sort(medianGray);
                        outputGray[i, j] = medianGray[medianPosition];
                    }
                }
                outputImage.setGray(outputGray);
            }

            return outputImage;
        }

        #endregion
    }
}
