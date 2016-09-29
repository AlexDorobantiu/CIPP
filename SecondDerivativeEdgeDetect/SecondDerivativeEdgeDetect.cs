using System;
using System.Collections.Generic;
using System.Text;
using ParametersSDK;
using Plugins.Filters;
using ProcessingImageSDK;

namespace SecondDerivativeEdgeDetect
{
    public class SecondDerivativeEdgeDetect : IFilter
    {

        public static List<IParameters> getParametersList()
        {
            return new List<IParameters>();
        }

        ProcessingImageSDK.ImageDependencies IFilter.getImageDependencies()
        {
            return new ImageDependencies(-1, -1, -1, -1);
        }

        ProcessingImageSDK.ProcessingImage IFilter.filter(ProcessingImageSDK.ProcessingImage inputImage)
        {
            //this should work only on grayscale images
            if (!inputImage.isGrayscale)
            {
                return inputImage;
            }

            const int laplacianOfGaussianSize = 5;
            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributesAndAlpha(inputImage);
            pi.addWatermark("Second derivative edge detect, c2015 Alexandru Dorobantiu");
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

            float[,] convolutedResult = new float[inputImage.getSizeY(), inputImage.getSizeX()];
            byte[,] outputGray = new byte[inputImage.getSizeY(), inputImage.getSizeX()];
            byte[,] inputImageGray = inputImage.getGray();

            for (int i = 0; i < pi.getSizeY() - (laplacianOfGaussianSize - 1); i++)
            {
                for (int j = 0; j < pi.getSizeX() - (laplacianOfGaussianSize - 1); j++)
                {
                    for (int k = 0; k < laplacianOfGaussianSize; k++)
                    {
                        for (int l = 0; l < laplacianOfGaussianSize; l++)
                        {
                            convolutedResult[i, j] += laplacianOfGaussian[k, l] * inputImageGray[i + k, j + l];
                        }
                    }
                }
            }

            for (int i = 1; i < pi.getSizeY() - 1; i++)
            {
                for (int j = 1; j < pi.getSizeX() - 1; j++)
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

            pi.setGray(outputGray);

            return pi;
        }
    }
}
