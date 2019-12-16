using System;
using System.Collections.Generic;
using ParametersSDK;
using Plugins.Filters;
using ProcessingImageSDK;
using System.IO;
using ProcessingImageSDK.Position;

namespace CannyFilter
{
    public class CannyFilter : IFilter
    {
        private int applyGauss;
        private float sigma;
        private int gradientType;
        private int thresholdHigh;
        private int thresholdLow;
        private int step;

        private static string[] yesNoEnumValues = { "Yes", "No" };
        private static string[] stepEnumValues = { "Gauss", "Gradient", "Angles", "Quantized Angles", "NMS", "Hyst Threshold" };
        private static string[] gradientTypeEnumValues = { "Sobel", "Scharr" };

        private static float[,] sobelX = new float[3, 3] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
        private static float[,] sobelY = new float[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };

        private static float[,] scharrX = new float[3, 3] { { 3, 10, 3 }, { 0, 0, 0 }, { -3, -10, -3 } };
        private static float[,] scharrY = new float[3, 3] { { -3, 0, 3 }, { -10, 0, 10 }, { -3, 0, 3 } };

        // delta for eight directions
        private static int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        private static int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>();
            parameters.Add(new ParametersEnum("Apply gauss:", 0, yesNoEnumValues, ParameterDisplayTypeEnum.listBox));
            parameters.Add(new ParametersFloat(0, 20, 1.4f, "Sigma:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersEnum("Gradient type:", 0, gradientTypeEnumValues, ParameterDisplayTypeEnum.listBox));
            parameters.Add(new ParametersInt32(0, 255, 16, "Threshold low:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersInt32(0, 255, 32, "Threshold high:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersEnum("Up to step:", 5, stepEnumValues, ParameterDisplayTypeEnum.listBox));

            return parameters;
        }

        public CannyFilter(int applyGauss, float sigma, int gradientType, int thresholdLow, int thresholdHigh, int step)
        {
            this.applyGauss = applyGauss;
            this.sigma = sigma;
            this.gradientType = gradientType;
            this.thresholdLow = thresholdLow;
            this.thresholdHigh = thresholdHigh;
            this.step = step;
        }

        public ImageDependencies getImageDependencies()
        {
            return null;
        }

        public float[,] generateNormalizedGaussConvolutionMatrix(float sigma, int size)
        {
            float[,] gaussConvolutionMatrix = new float[size, size];

            float coef1 = (float)(1 / (2 * Math.PI * sigma * sigma));
            float coef2 = -1 / (2 * sigma * sigma);
            int min = size / 2;
            int max = size / 2 + size % 2;

            for (int y = -min; y < max; y++)
            {
                for (int x = -min; x < max; x++)
                {
                    gaussConvolutionMatrix[y + min, x + min] = coef1 * (float)Math.Exp(coef2 * (x * x + y * y));
                }
            }
            return ProcessingImageUtils.normalize(gaussConvolutionMatrix);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage pi = new ProcessingImage();
            pi.initialize(Path.ChangeExtension(inputImage.getName(), ".png"), inputImage.getSizeX(), inputImage.getSizeY());
            pi.addWatermark("Canny Filter sigma: " + sigma.ToString("0.0") + " Gradien: " + gradientTypeEnumValues[gradientType] +
                " TL: " + thresholdLow + " TH: " + thresholdHigh + " Step: " + stepEnumValues[step] + " v1.0, Alexandru Dorobanțiu");

            int imageSizeX = pi.getSizeX();
            int imageSizeY = pi.getSizeY();
            byte[,] inputGray = inputImage.getGray();

            // 1. Gauss
            float[,] gaussResult;
            // Yes option is index 0
            if (applyGauss == 0)
            {
                float[,] gaussConvolutionMatrix = generateNormalizedGaussConvolutionMatrix(sigma, 5);
                gaussResult = ProcessingImageUtils.mirroredMarginConvolution(inputGray, gaussConvolutionMatrix);
            }
            else
            {
                gaussResult = ProcessingImageUtils.convertToFloat(inputGray);
            }

            // 2. Gradient
            float[,] gradientFilterX;
            float[,] gradientFilterY;
            if (gradientType == 0)
            {
                gradientFilterX = ProcessingImageUtils.semiNormalize(sobelX);
                gradientFilterY = ProcessingImageUtils.semiNormalize(sobelY);
            }
            else
            {
                gradientFilterX = ProcessingImageUtils.semiNormalize(scharrX);
                gradientFilterY = ProcessingImageUtils.semiNormalize(scharrY);
            }
            float[,] gradientX = ProcessingImageUtils.mirroredMarginConvolution(gaussResult, gradientFilterX);
            float[,] gradientY = ProcessingImageUtils.mirroredMarginConvolution(gaussResult, gradientFilterY);

            // 3. Gradient Amplitude
            float[,] amplitudeResult = new float[imageSizeY, imageSizeX];
            for (int i = 0; i < imageSizeY; i++)
            {
                for (int j = 0; j < imageSizeX; j++)
                {
                    amplitudeResult[i, j] = (float)Math.Sqrt(gradientX[i, j] * gradientX[i, j] + gradientY[i, j] * gradientY[i, j]);
                }
            }

            // 4. Angle of gradient
            float[,] anglesResult = new float[imageSizeY, imageSizeX];
            for (int i = 0; i < imageSizeY; i++)
            {
                for (int j = 0; j < imageSizeX; j++)
                {
                    anglesResult[i, j] = (float)Math.Atan2(gradientX[i, j], gradientY[i, j]);
                }
            }

            // 5. Quantized angles
            int[,] quantizedAngles = new int[imageSizeY, imageSizeX];
            for (int i = 0; i < imageSizeY; i++)
            {
                for (int j = 0; j < imageSizeX; j++)
                {
                    float angle = anglesResult[i, j];
                    if ((angle <= (5 * Math.PI) / 8 && angle > (3 * Math.PI) / 8) || (angle > -(5 * Math.PI) / 8 && angle <= -(3 * Math.PI) / 8))
                    {
                        quantizedAngles[i, j] = 1;
                    }
                    else
                    {
                        if (angle <= (3 * Math.PI) / 8 && angle > Math.PI / 8 || angle > -(7 * Math.PI) / 8 && angle <= -(5 * Math.PI) / 8)
                        {
                            quantizedAngles[i, j] = 2;
                        }
                        else
                        {
                            if (angle <= (7 * Math.PI / 8) && angle > (5 * Math.PI / 8) || angle > -(3 * Math.PI) / 8 && angle < -(Math.PI / 8))
                            {
                                quantizedAngles[i, j] = 3;
                            }
                            else
                            {
                                quantizedAngles[i, j] = 4;
                            }
                        }
                    }
                }
            }

            // 6. Non maximal suppresion
            float[,] nmsResult = new float[imageSizeY, imageSizeX];
            for (int i = 0; i < imageSizeY; i++)
            {
                for (int j = 0; j < imageSizeX; j++)
                {
                    switch (quantizedAngles[i, j])
                    {
                        case 1:
                            if ((i == 0 || amplitudeResult[i, j] >= amplitudeResult[i - 1, j]) &&
                                (i == imageSizeY - 1 || amplitudeResult[i, j] > amplitudeResult[i + 1, j]))
                            {
                                nmsResult[i, j] = amplitudeResult[i, j];
                            }
                            break;
                        case 2:
                            if ((i == 0 || j == imageSizeX - 1 || amplitudeResult[i, j] >= amplitudeResult[i - 1, j + 1]) &&
                                (i == imageSizeY - 1 || j == 0 || amplitudeResult[i, j] > amplitudeResult[i + 1, j - 1]))
                            {
                                nmsResult[i, j] = amplitudeResult[i, j];
                            }
                            break;
                        case 3:
                            if ((i == 0 || j == 0 || amplitudeResult[i, j] >= amplitudeResult[i - 1, j - 1]) &&
                                (i == imageSizeY - 1 || j == imageSizeX - 1 || amplitudeResult[i, j] > amplitudeResult[i + 1, j + 1]))
                            {
                                nmsResult[i, j] = amplitudeResult[i, j];
                            }
                            break;
                        case 4:
                            if ((j == 0 || amplitudeResult[i, j] >= amplitudeResult[i, j - 1]) &&
                                (j == imageSizeX - 1 || amplitudeResult[i, j] > amplitudeResult[i, j + 1]))
                            {
                                nmsResult[i, j] = amplitudeResult[i, j];
                            }
                            break;
                    }
                }
            }

            // 7. Hysteresis thresolding
            float[,] hysteresisResult = new float[imageSizeY, imageSizeX];
            bool[,] retainedPositions = applyHysteresisThreshold(nmsResult, imageSizeX, imageSizeY);

            for (var i = 0; i < imageSizeY; i++)
            {
                for (var j = 0; j < imageSizeX; j++)
                {
                    if (retainedPositions[i, j])
                    {
                        hysteresisResult[i, j] = nmsResult[i, j];
                    }
                }
            }

            // Setup image to show
            switch (step)
            {
                case 0:
                    pi.setGray(ProcessingImageUtils.truncateToDisplay(gaussResult));
                    break;
                case 1:
                    pi.setGray(ProcessingImageUtils.truncateToDisplay(amplitudeResult));
                    break;
                case 2:
                    pi.setGray(convertAnglesToBytesForPreview(imageSizeY, imageSizeX, anglesResult));
                    break;
                case 3:
                    pi.setGray(convertQuantizedAnglesToBytesForPreview(imageSizeY, imageSizeX, quantizedAngles));
                    break;
                case 4:
                    pi.setGray(ProcessingImageUtils.truncateToDisplay(nmsResult));
                    break;
                case 5:
                    pi.setGray(ProcessingImageUtils.truncateToDisplay(hysteresisResult));
                    break;
                default:
                    break;
            }

            return pi;
        }


        private bool[,] applyHysteresisThreshold(float[,] nmsResult, int sizeX, int sizeY)
        {
            bool[,] retained = new bool[sizeY, sizeX];
            bool[,] visited = new bool[sizeY, sizeX];
            Queue<Position2d> positionsQueue = new Queue<Position2d>();
            Position2d currentPosition;
            Position2d nextPosition;
            for (var i = 0; i < sizeY; i++)
            {
                for (var j = 0; j < sizeX; j++)
                {
                    if (nmsResult[i, j] >= thresholdHigh)
                    {
                        retained[i, j] = true;

                        currentPosition.x = j;
                        currentPosition.y = i;
                        positionsQueue.Enqueue(currentPosition);

                        while (positionsQueue.Count > 0)
                        {
                            currentPosition = positionsQueue.Dequeue();
                            for (int deltaIndex = 0; deltaIndex < 8; deltaIndex++)
                            {
                                nextPosition.x = currentPosition.x + dx[deltaIndex];
                                nextPosition.y = currentPosition.y + dy[deltaIndex];
                                if (nextPosition.y >= 0 && nextPosition.x >= 0 && nextPosition.y < sizeY && nextPosition.x < sizeX)
                                {
                                    if (!visited[nextPosition.y, nextPosition.x] &&
                                        (nmsResult[nextPosition.y, nextPosition.x] >= thresholdLow && nmsResult[nextPosition.y, nextPosition.x] < thresholdHigh))
                                    {
                                        visited[nextPosition.y, nextPosition.x] = true;
                                        retained[nextPosition.y, nextPosition.x] = true;
                                        positionsQueue.Enqueue(nextPosition);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return retained;
        }

        private byte[,] convertAnglesToBytesForPreview(int newSizeY, int newSizeX, float[,] angles)
        {
            byte[,] result = new byte[newSizeY, newSizeX];
            for (int i = 0; i < newSizeY; i++)
            {
                for (int j = 0; j < newSizeX; j++)
                {
                    result[i, j] = (byte)(((angles[i, j] + Math.PI / 2) / Math.PI * 255) + 0.5);
                }
            }

            return result;
        }

        private byte[,] convertQuantizedAnglesToBytesForPreview(int newSizeY, int newSizeX, int[,] quantizedAngles)
        {
            byte[,] result = new byte[newSizeY, newSizeX];
            for (int i = 0; i < newSizeY; i++)
            {
                for (int j = 0; j < newSizeX; j++)
                {
                    switch(quantizedAngles[i, j])
                    {
                        case 1:
                            result[i, j] = 1;
                            break;
                        case 2:
                            result[i, j] = 64;
                            break;
                        case 3:
                            result[i, j] = 128;
                            break;
                        case 4:
                            result[i, j] = 255;
                            break;
                    }
                    
                }
            }

            return result;
        }

    }
}
