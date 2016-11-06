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
        private static string[] intervalEnumValues = { "stretch", "truncate" };

        private static readonly List<IParameters> parameters = new List<IParameters>();

        static GaborFilter()
        {
            parameters.Add(new ParametersInt32(3, 100, 8, "Size:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(1, 100, 8, "Wavelength:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(0, 3.141592f, 0, "Orientation:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(0, 3.141592f, 1.5707f, "Phase:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(0, 100, 2, "Bandwidth:", DisplayType.textBox));
            parameters.Add(new ParametersFloat(0, 100, 1, "Aspect Ratio:", DisplayType.textBox));
            parameters.Add(new ParametersEnum("Interval:", 0, intervalEnumValues, DisplayType.comboBox));
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int filterSize;
        private float wavelength;
        private float orientation;
        private float phase;
        private float bandwidth;
        private float aspectRatio;
        private int intervalType;

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
            float[,] gaborFilterMatrix = new float[filterSize, filterSize];
            float sigma = (float)(wavelength * (1 / Math.PI * Math.Sqrt(Math.Log(2) / 2) * ((Math.Pow(2, bandwidth) + 1) / (Math.Pow(2, bandwidth) - 1))));

            for (int x = 0; x < filterSize; x++)
            {
                for (int y = 0; y < filterSize; y++)
                {
                    double primeX = (x - filterSize / 2.0) * Math.Cos(orientation) + (y - filterSize / 2.0) * Math.Sin(orientation);
                    double primeY = -(x - filterSize / 2.0) * Math.Sin(orientation) + (y - filterSize / 2.0) * Math.Cos(orientation);

                    double result = Math.Exp(-(primeX * primeX + aspectRatio * aspectRatio * primeY * primeY) / (2 * sigma * sigma))
                                  * Math.Cos(2 * Math.PI * primeX / wavelength + phase);
                    gaborFilterMatrix[y, x] = (float)result;
                }
            }

            ProcessingImage outputImage;
            if (intervalType == 0)
            {
                outputImage = new ProcessingImage();
                outputImage.copyAttributesAndAlpha(inputImage);
                if (inputImage.grayscale)
                {                    
                    float[,] filteredImage = ProcessingImageUtils.mirroredMarginConvolution(inputImage.getGray(), gaborFilterMatrix);
                    byte[,] outputGray = ProcessingImageUtils.fitHistogramToDisplay(filteredImage);
                    outputImage.setGray(outputGray);
                }
                else
                {
                    float[,] filteredImageR = ProcessingImageUtils.mirroredMarginConvolution(inputImage.getRed(), gaborFilterMatrix);
                    float[,] filteredImageG = ProcessingImageUtils.mirroredMarginConvolution(inputImage.getGreen(), gaborFilterMatrix);
                    float[,] filteredImageB = ProcessingImageUtils.mirroredMarginConvolution(inputImage.getBlue(), gaborFilterMatrix);
                    byte[,] red = ProcessingImageUtils.fitHistogramToDisplay(filteredImageR);
                    byte[,] green = ProcessingImageUtils.fitHistogramToDisplay(filteredImageG);
                    byte[,] blue = ProcessingImageUtils.fitHistogramToDisplay(filteredImageB);
                    outputImage.setRed(red);
                    outputImage.setGreen(green);
                    outputImage.setBlue(blue);
                }
            }
            else
            {
                outputImage = inputImage.mirroredMarginConvolution(gaborFilterMatrix);
            }
            outputImage.addWatermark("Gabor Filter, size:" + filterSize + ", wavelength:" + wavelength + ", orientation:" + orientation + ", phase:" + phase
                + ", bandwidth:" + bandwidth + ", aspect ratio:" + aspectRatio + ", interval:" + intervalEnumValues[intervalType] + " v1.0, Alex Dorobantiu");
            return outputImage;
        }

        #endregion
    }
}