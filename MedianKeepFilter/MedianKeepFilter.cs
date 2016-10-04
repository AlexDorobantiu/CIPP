using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace Plugins.Filters.MedianKeepFilter
{
    public class MedianKeepFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();
        static MedianKeepFilter()
        {
            parameters.Add(new ParametersInt32(1, 32, 1, "Ordin:", DisplayType.textBox));
        }
        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int order;
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
            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributesAndAlpha(inputImage);
            pi.addWatermark("Median Keep Filter, order: " + order + " v1.0, Alex Dorobantiu");

            int medianSize = (2 * order + 1) * (2 * order + 1);

            if (!inputImage.grayscale)
            {
                byte[,] r = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] g = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
                byte[,] b = new byte[inputImage.getSizeY(), inputImage.getSizeX()];

                byte[,] ir = inputImage.getRed();
                byte[,] ig = inputImage.getGreen();
                byte[,] ib = inputImage.getBlue();
                byte[,] iy = inputImage.getLuminance();
                
                byte[] median = new byte[medianSize];

                for (int i = order; i < pi.getSizeY() - order; i++)
                {
                    for (int j = order; j < pi.getSizeX() - order; j++)
                    {
                        int pivot = 0;
                        for (int k = i - order; k <= i + order; k++)
                            for (int l = j - order; l <= j + order; l++)
                                median[pivot++] = iy[k, l];
                        Array.Sort(median);
                        byte y = median[medianSize / 2];
                        for (int k = i - order; k <= i + order; k++)
                            for (int l = j - order; l <= j + order; l++)
                                if (iy[k, l] == y)
                                {
                                    r[i, j] = ir[k, l];
                                    g[i, j] = ig[k, l];
                                    b[i, j] = ib[k, l];
                                    break;
                                }
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
