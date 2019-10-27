using System;
using System.Collections.Generic;
using Plugins.Filters;
using ParametersSDK;
using ProcessingImageSDK;
using System.IO;
using ProcessingImageSDK.Position;

namespace KirschFilter
{
    public class KirschFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();

        private static List<float[,]> templates = new List<float[,]> 
        {
           new float[,] {{ -3 / 15.0f, -3 / 15.0f, 5 / 15.0f }, { -3 / 15.0f, 0, 5 / 15.0f }, { -3 / 15.0f, -3 / 15.0f, 5 / 15.0f } }, // -
           new float[,] {{ -3 / 15.0f, 5 / 15.0f, 5 / 15.0f }, { -3 / 15.0f, 0, 5 / 15.0f }, { -3 / 15.0f, -3 / 15.0f, -3 / 15.0f } }, // /
           new float[,] {{ 5 / 15.0f, 5 / 15.0f, 5 / 15.0f }, { -3 / 15.0f, 0, -3 / 15.0f }, { -3 / 15.0f, -3 / 15.0f, -3 / 15.0f } }, // |
           new float[,] {{ 5 / 15.0f, 5 / 15.0f, -3 / 15.0f }, { 5 / 15.0f, 0, -3 / 15.0f }, { -3 / 15.0f, -3 / 15.0f, -3 / 15.0f } }, // \
           new float[,] {{ 5 / 15.0f, -3 / 15.0f, -3 / 15.0f }, { 5 / 15.0f, 0, -3 / 15.0f }, { 5 / 15.0f, -3 / 15.0f, -3 / 15.0f } }, // -
           new float[,] {{ -3 / 15.0f, -3 / 15.0f, -3 / 15.0f }, { 5 / 15.0f, 0, -3 / 15.0f }, { 5 / 15.0f, 5 / 15.0f, -3 / 15.0f } }, // /
           new float[,] {{ -3 / 15.0f, -3 / 15.0f, -3 / 15.0f }, { -3 / 15.0f, 0, -3 / 15.0f }, { 5 / 15.0f, 5 / 15.0f, 5 / 15.0f } }, // |
           new float[,] {{ -3 / 15.0f, -3 / 15.0f, -3 / 15.0f }, { -3 / 15.0f, 0, 5 / 15.0f }, { -3 / 15.0f, 5 / 15.0f, 5 / 15.0f } }  // \
        };

        // delta for eight directions
        private static int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        private static int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

        private static string[] yesNoEnumValues = { "Yes", "No" };
        private static string[] stepEnumValues = { "Gauss", "Max Gradient", "Angles", "NMS", "Hyst Threshold" };

        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>();

            parameters.Add(new ParametersEnum("Apply gauss:", 0, yesNoEnumValues, ParameterDisplayTypeEnum.listBox));
            parameters.Add(new ParametersFloat(0, 20, 1.4f, "Sigma:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersInt32(0, 255, 0, "Threshold low:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersInt32(0, 255, 32, "Threshold high:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersEnum("Up to step:", 4, stepEnumValues, ParameterDisplayTypeEnum.listBox));

            return parameters;
        }

        private int applyGauss;
        private float sigma;
        private int thresholdHigh;
        private int thresholdLow;
        private int step;

        public KirschFilter(int applyGauss, float sigma, int thresholdLow, int thresholdHigh, int step)
        {
            this.applyGauss = applyGauss;
            this.sigma = sigma;
            this.thresholdLow = thresholdLow;
            this.thresholdHigh = thresholdHigh;
            this.step = step;
        }

        #region IFilter Members

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

            ProcessingImageUtils.normalize(gaussConvolutionMatrix);
            return gaussConvolutionMatrix;
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage pi = new ProcessingImage();
            pi.initialize(Path.ChangeExtension(inputImage.getName(), ".png"), inputImage.getSizeX(), inputImage.getSizeY());
            pi.addWatermark("Kirsch Filter sigma: " + sigma.ToString("0.0") + " TL: " + thresholdLow + " TH: " + thresholdHigh + " Step: " + stepEnumValues[step] + " v1.0, Alexandru Dorobanțiu");

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

            // 2.1 Gradient
            List<float[,]> results = new List<float[,]>(templates.Count);
            foreach (float[,] template in templates)
            {
                results.Add(ProcessingImageUtils.mirroredMarginConvolution(gaussResult, template));
            }

            // 2.2 + 3 Max Gradient + Angles
            float[,] amplitudeResult = new float[imageSizeY, imageSizeX];
            int[,] anglesResult = new int[imageSizeY, imageSizeX];
            for (int i = 0; i < imageSizeY; i++)
            {
                for (int j = 0; j < imageSizeX; j++)
                {
                    int direction = 0;
                    float maxValue = 0;
                    for (int templateIndex = 0; templateIndex < templates.Count; templateIndex++)
                    {
                        float value = results[templateIndex][i, j];
                        if (value > maxValue)
                        {
                            maxValue = value;
                            direction = templateIndex;
                        }
                    }
                    amplitudeResult[i, j] = maxValue;
                    anglesResult[i, j] = direction;
                }
            }

            // 4. Non maximal suppresion
            float[,] nmsResult = new float[imageSizeY, imageSizeX];
            for (int i = 0; i < imageSizeY; i++)
            {
                for (int j = 0; j < imageSizeX; j++)
                {
                    int angle = anglesResult[i, j];
                    if (angle == 2 || angle == 6)
                    {
                        if ((i == 0 || amplitudeResult[i, j] >= amplitudeResult[i - 1, j]) &&
                            (i == imageSizeY - 1 || amplitudeResult[i, j] > amplitudeResult[i + 1, j]))
                        {
                            nmsResult[i, j] = amplitudeResult[i, j];
                        }
                    }
                    else
                    {
                        if (angle == 1 || angle == 5)
                        {
                            if ((i == 0 || j == imageSizeX - 1 || amplitudeResult[i, j] >= amplitudeResult[i - 1, j + 1]) &&
                                (i == imageSizeY - 1 || j == 0 || amplitudeResult[i, j] > amplitudeResult[i + 1, j - 1]))
                            {
                                nmsResult[i, j] = amplitudeResult[i, j];
                            }
                        }
                        else
                        {
                            if (angle == 3 || angle == 7)
                            {
                                if ((i == 0 || j == 0 || amplitudeResult[i, j] >= amplitudeResult[i - 1, j - 1]) &&
                                    (i == imageSizeY - 1 || j == imageSizeX - 1 || amplitudeResult[i, j] > amplitudeResult[i + 1, j + 1]))
                                {
                                    nmsResult[i, j] = amplitudeResult[i, j];
                                }
                            }
                            else
                            {
                                if ((j == 0 || amplitudeResult[i, j] >= amplitudeResult[i, j - 1]) &&
                                    (j == imageSizeX - 1 || amplitudeResult[i, j] > amplitudeResult[i, j + 1]))
                                {
                                    nmsResult[i, j] = amplitudeResult[i, j];
                                }
                            }
                        }
                    }
                }
            }

            // 5. Hysteresis thresolding
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
                    pi.setGray(ProcessingImageUtils.truncateToDisplay(nmsResult));
                    break;
                case 4:
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

        private byte[,] convertAnglesToBytesForPreview(int newSizeY, int newSizeX, int[,] angles)
        {
            byte[,] result = new byte[newSizeY, newSizeX];
            for (int i = 0; i < newSizeY; i++)
            {
                for (int j = 0; j < newSizeX; j++)
                {
                    result[i, j] = (byte)(Math.Min(angles[i, j] * 32, 255));
                }
            }

            return result;
        }

        #endregion

    }
}
