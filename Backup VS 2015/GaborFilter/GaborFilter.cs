using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace Plugins.Filters.GaborFilter
{
    public class GaborFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();
        static GaborFilter()
        {
            parameters.Add(new ParametersInt32(3, 100, 8, "Size:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(1, 100, 8, "Wavelength:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(0, 3.141592f, 0, "Orientation:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(0, 3.141592f, 1.5707f, "Phase:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(0, 100, 2, "Bandwidth:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(0, 100, 1, "Aspect Ratio:", DisplayType.textBox));
            string[] values = { "stretch", "truncate" };
            parameters.Add(new ParametersEnum("Interval:", 0, values, DisplayType.comboBox));
        }
        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        int filterSize;
        float wavelength;
        float orientation;
        float phase;
        float bandwidth;
        float aspectRatio;
        int intervalType;

        public GaborFilter(int filterSize, float wavelength, float orientation, float phase, float bandwidth, float aspectRatio, int intervalType)
        {
            this.filterSize = filterSize;
            this.wavelength = wavelength;
            this.orientation = orientation;
            this.phase = phase;
            this.bandwidth = bandwidth;
            this.aspectRatio = aspectRatio;
            this.intervalType = intervalType;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(-1, -1, -1, -1);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            float[,] filter = new float[filterSize, filterSize];
            float sigma = (float)(wavelength * (1 / Math.PI * Math.Sqrt(Math.Log(2) / 2) * ((Math.Pow(2, bandwidth) + 1) / (Math.Pow(2, bandwidth) - 1))));

            for (int x = 0; x < filterSize; x++)
            {
                for (int y = 0; y < filterSize; y++)
                {
                    double primeX = (x - filterSize / 2.0) * Math.Cos(orientation) + (y - filterSize / 2.0) * Math.Sin(orientation);
                    double primeY = -(x - filterSize / 2.0) * Math.Sin(orientation) + (y - filterSize / 2.0) * Math.Cos(orientation);

                    double result = Math.Exp(-(primeX * primeX + aspectRatio * aspectRatio * primeY * primeY) / (2 * sigma * sigma))
                                  * Math.Cos(2 * Math.PI * primeX / wavelength + phase);
                    filter[y, x] = (float)(result);
                }
            }

            if (intervalType == 0)
            {
                ProcessingImage pi = new ProcessingImage();
                pi.copyAttributesAndAlpha(inputImage);
                pi.addWatermark("Gabor Filter, size:" + filterSize + ", wavelength:" + wavelength + ", orientation:" + orientation + ", phase:" + phase + ", bandwidth:" + bandwidth + ", aspect ratio:" + aspectRatio + " v1.0 Alex Dorobantiu");
                
                int imageYSize = pi.getSizeY();
                int imageXSize = pi.getSizeX();

                if (inputImage.isGrayscale)
                {
                    int[,] filteredImage = new int[imageYSize, imageXSize];
                    byte[,] g = new byte[imageYSize, imageXSize];
                    byte[,] ig = inputImage.getGray();

                    int max = int.MinValue;
                    int min = int.MaxValue;

                    for (int i = filterSize - 1; i < imageYSize; i++)
                    {
                        for (int j = filterSize - 1; j < imageXSize; j++)
                        {
                            float sum = 0;
                            for (int k = filterSize - 1; k >= 0; k--)
                                for (int l = filterSize - 1; l >= 0; l--)
                                    sum += ig[i - k, j - l] * filter[k, l];

                            if (sum > max) max = (int)sum;
                            if (sum < min) min = (int)sum;
                            filteredImage[i, j] = (int)sum;
                        }
                    }

                    for (int i = filterSize - 1; i < imageYSize; i++)
                        for (int j = filterSize - 1; j < imageXSize; j++)
                            g[i, j] = (byte)(((filteredImage[i, j] - min) * 255) / (max - min));

                    pi.setGray(g);
                }
                else
                {
                    int[,] filteredImageR = new int[imageYSize, imageXSize];
                    int[,] filteredImageG = new int[imageYSize, imageXSize];
                    int[,] filteredImageB = new int[imageYSize, imageXSize];
                    byte[,] red = new byte[imageYSize, imageXSize];
                    byte[,] green = new byte[imageYSize, imageXSize];
                    byte[,] blue = new byte[imageYSize, imageXSize];
                    byte[,] ir = inputImage.getRed();
                    byte[,] ig = inputImage.getGreen();
                    byte[,] ib = inputImage.getBlue();

                    int maxR = int.MinValue;
                    int minR = int.MaxValue;
                    int maxG = int.MinValue;
                    int minG = int.MaxValue;
                    int maxB = int.MinValue;
                    int minB = int.MaxValue;

                    for (int i = filterSize - 1; i < imageYSize; i++)
                    {
                        for (int j = filterSize - 1; j < imageXSize; j++)
                        {
                            float sumR = 0;
                            float sumG = 0;
                            float sumB = 0;
                            for (int k = filterSize - 1; k >= 0; k--)
                                for (int l = filterSize - 1; l >= 0; l--)
                                {
                                    sumR += ir[i - k, j - l] * filter[k, l];
                                    sumG += ig[i - k, j - l] * filter[k, l];
                                    sumB += ib[i - k, j - l] * filter[k, l];
                                }
                            if (sumR > maxR) maxR = (int)sumR;
                            if (sumR < minR) minR = (int)sumR;
                            filteredImageR[i, j] = (int)sumR;

                            if (sumG > maxG) maxG = (int)sumG;
                            if (sumG < minG) minG = (int)sumG;
                            filteredImageG[i, j] = (int)sumG;

                            if (sumB > maxB) maxB = (int)sumB;
                            if (sumB < minB) minB = (int)sumB;
                            filteredImageB[i, j] = (int)sumB;
                        }
                    }

                    for (int i = filterSize - 1; i < imageYSize; i++)
                        for (int j = filterSize - 1; j < imageXSize; j++)
                        {
                            red[i, j] = (byte)(((filteredImageR[i, j] - minR) * 255) / (maxR - minR));
                            green[i, j] = (byte)(((filteredImageG[i, j] - minG) * 255) / (maxG - minG));
                            blue[i, j] = (byte)(((filteredImageB[i, j] - minB) * 255) / (maxB - minB));
                        }
                    pi.setRed(red);
                    pi.setGreen(green);
                    pi.setBlue(blue);
                }

                return pi;
            }
            else
            {
                ProcessingImage pi = inputImage.convolution(filter);
                pi.addWatermark("Gabor Filter, size:" + filterSize + ", wavelength:" + wavelength + ", orientation:" + orientation + ", phase:" + phase + ", bandwidth:" + bandwidth + ", aspect ratio:" + aspectRatio + " v1.0 Alex Dorobantiu");
                return pi;
            }
        }

        #endregion
    }
}