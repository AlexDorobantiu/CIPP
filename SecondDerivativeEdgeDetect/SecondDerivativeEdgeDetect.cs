using System;
using System.Collections.Generic;
using System.Text;
using ParametersSDK;
using Plugins.Filters;
using ProcessingImageSDK;

namespace Plugins.Filters.SecondDerivativeEdgeDetect
{
    public class SecondDerivativeEdgeDetect : IFilter
    {
        public static List<IParameters> getParametersList()
        {
            return new List<IParameters>();
        }

        public SecondDerivativeEdgeDetect()
        {
        }

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(-1, -1, -1, -1);
        }

        public ProcessingImage filter(ProcessingImageSDK.ProcessingImage inputImage)
        {
            const int laplacianOfGaussianSize = 5;
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark("Second derivative edge detect, v1.0, Alex Dorobantiu");

            float[,] laplacianOfGaussian = new float[laplacianOfGaussianSize, laplacianOfGaussianSize];

            laplacianOfGaussian[0, 0] = 0;
            laplacianOfGaussian[0, 1] = 0;
            laplacianOfGaussian[0, 2] = -1;
            laplacianOfGaussian[0, 3] = 0;
            laplacianOfGaussian[0, 4] = 0;

            laplacianOfGaussian[1, 0] = 0;
            laplacianOfGaussian[1, 1] = -1;
            laplacianOfGaussian[1, 2] = -2;
            laplacianOfGaussian[1, 3] = -1;
            laplacianOfGaussian[1, 4] = 0;

            laplacianOfGaussian[2, 0] = -1;
            laplacianOfGaussian[2, 1] = -1;
            laplacianOfGaussian[2, 2] = 16;
            laplacianOfGaussian[2, 3] = -2;
            laplacianOfGaussian[2, 4] = 1;

            laplacianOfGaussian[3, 0] = 0;
            laplacianOfGaussian[3, 1] = -1;
            laplacianOfGaussian[3, 2] = -2;
            laplacianOfGaussian[3, 3] = -1;
            laplacianOfGaussian[3, 4] = -0;

            laplacianOfGaussian[4, 0] = 0;
            laplacianOfGaussian[4, 1] = 0;
            laplacianOfGaussian[4, 2] = -1;
            laplacianOfGaussian[4, 3] = 0;
            laplacianOfGaussian[4, 4] = 0;

            float[,] convolutedResult = ProcessingImageUtils.mirroredMarginConvolution(inputImage.getGray(), laplacianOfGaussian);
            byte[,] outputGray = new byte[inputImage.getSizeY(), inputImage.getSizeX()];

            for (int i = 1; i < outputImage.getSizeY() - 1; i++)
            {
                for (int j = 1; j < outputImage.getSizeX() - 1; j++)
                {
                    if (convolutedResult[i, j] < 0)
                    {
                        if ((convolutedResult[i - 1, j] > 0) || (convolutedResult[i + 1, j] > 0) || (convolutedResult[i, j + 1] > 0) || (convolutedResult[i, j - 1] > 0) ||
                            (convolutedResult[i - 1, j - 1] > 0) || (convolutedResult[i + 1, j - 1] > 0) || (convolutedResult[i - 1, j + 1] > 0) || (convolutedResult[i + 1, j + 1] > 0))
                        {
                            outputGray[i, j] = 255;
                        }
                        else
                        {
                            outputGray[i, j] = 0;
                        }
                    }
                    else if (convolutedResult[i, j] >= 0)
                    {
                        if ((convolutedResult[i - 1, j] < 0) || (convolutedResult[i + 1, j] < 0) || (convolutedResult[i, j + 1] < 0) || (convolutedResult[i, j - 1] < 0) ||
                            (convolutedResult[i - 1, j - 1] < 0) || (convolutedResult[i + 1, j - 1] < 0) || (convolutedResult[i - 1, j + 1] < 0) || (convolutedResult[i + 1, j + 1] < 0))
                        {
                            outputGray[i, j] = 255;
                        }
                        else
                        {
                            outputGray[i, j] = 0;
                        }
                    }
                }
            }

            outputImage.setGray(outputGray);
            return outputImage;
        }
    }
}