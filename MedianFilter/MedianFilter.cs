using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace Plugins.Filters.MedianFilter
{
    public class MedianFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();
        static MedianFilter()
        {
            parameters.Add(new ParametersInt32(1, 5, 1, "Ordin:", DisplayType.trackBar));
        }
        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int order;
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
            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributesAndAlpha(inputImage);
            pi.addWatermark("Median Filter, order: " + order + " v1.0, Alex Dorobantiu");

            int medianSize = (2 * order + 1) * (2 * order + 1);

            if (!inputImage.isGrayscale)
            {
                byte[,] r = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] g = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] b = new byte[inputImage.getSizeY(), inputImage.getSizeX()];

                byte[,] ir = inputImage.getRed();
                byte[,] ig = inputImage.getGreen();
                byte[,] ib = inputImage.getBlue();

                
                byte[] medianR = new byte[medianSize];
                byte[] medianG = new byte[medianSize];
                byte[] medianB = new byte[medianSize];

                for (int i = order; i < pi.getSizeY() - order; i++)
                {
                    for (int j = order; j < pi.getSizeX() - order; j++)
                    {
                        int pivot = 0;
                        for (int k = i - order; k <= i + order; k++)
                        {
                            for (int l = j - order; l <= j + order; l++)
                            {
                                medianR[pivot] = ir[k, l];
                                medianG[pivot] = ig[k, l];
                                medianB[pivot++] = ib[k, l];
                            }
                        }
                        Array.Sort(medianR);
                        Array.Sort(medianG);
                        Array.Sort(medianB);

                        r[i, j] = medianR[medianSize / 2];
                        g[i, j] = medianG[medianSize / 2];
                        b[i, j] = medianB[medianSize / 2];
                    }
                }
                pi.setRed(r);
                pi.setGreen(g);
                pi.setBlue(b);
            }
            else
            {
                byte[,] gray = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] ig = inputImage.getGray();

                byte[] medianGray = new byte[medianSize];
                for (int i = order; i < pi.getSizeY() - order; i++)
                {
                    for (int j = order; j < pi.getSizeX() - order; j++)
                    {
                        int pivot = 0;
                        for (int k = i - order; k <= i + order; k++)
                            for (int l = j - order; l <= j + order; l++)
                                medianGray[pivot++] = ig[k, l];
                        Array.Sort(medianGray);
                        gray[i, j] = medianGray[medianSize / 2];
                    }
                }
                pi.setGray(gray);
            }

            return pi;
        }

        #endregion
    }
}
