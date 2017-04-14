using System;
using System.Collections.Generic;
using System.Text;
using Plugins.Filters;
using ParametersSDK;
using ProcessingImageSDK;
using System.IO;

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

        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>();
            parameters.Add(new ParametersFloat(0, 20, 1.4f, "Sigma:", DisplayType.textBox));
            parameters.Add(new ParametersInt32(0, 255, 0, "Threshold low:", DisplayType.textBox));
            parameters.Add(new ParametersInt32(0, 255, 32, "Threshold high:", DisplayType.textBox));

            return parameters;
        }

        private float sigma;
        private int thresholdHigh;
        private int thresholdLow;

        public KirschFilter(float sigma, int thresholdLow, int thresholdHigh)
        {
            this.sigma = sigma;
            this.thresholdLow = thresholdLow;
            this.thresholdHigh = thresholdHigh;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(1, 1, 1, 1);
        }

        public float[,] generateNormalizedGaussConvolutionMatrix(float sigma, int size)
        {
            float[,] gaussConvolutionMatrix = new float[size, size];

            float coef1 = (float)(1 / (2 * Math.PI * sigma * sigma));
            float coef2 = -1 / (2 * sigma * sigma);
            int min = size / 2;
            int max = size / 2 + size % 2;

            float sum = 0;
            for (int y = -min; y < max; y++)
            {
                for (int x = -min; x < max; x++)
                {
                    sum += gaussConvolutionMatrix[y + min, x + min] = coef1 * (float)Math.Exp(coef2 * (x * x + y * y));
                }
            }

            // normalize
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    gaussConvolutionMatrix[y, x] /= sum;
                }
            }
            return gaussConvolutionMatrix;
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage pi = new ProcessingImage();
            pi.initialize(Path.ChangeExtension(inputImage.getName(), ".png"), inputImage.getSizeX(), inputImage.getSizeY());
            pi.addWatermark("Kirsch Filter sigma: " + sigma.ToString("0.0") + " TL: " + thresholdLow + " TH: " + thresholdHigh + " v1.0, Alexandru Dorobanțiu");

            int imageSizeX = pi.getSizeX();
            int imageSizeY = pi.getSizeY();
            byte[,] inputGray = inputImage.getGray();

            float[,] gaussConvolutionMatrix = generateNormalizedGaussConvolutionMatrix(sigma, 5);
            float[,] gaussResult = ProcessingImageUtils.mirroredMarginConvolution(inputGray, gaussConvolutionMatrix);
            List<float[,]> results = new List<float[,]>(templates.Count);
            foreach (float[,] template in templates)
            {
                results.Add(ProcessingImageUtils.mirroredMarginConvolution(gaussResult, template));
            }

            float[,] amplitudeResult = new float[imageSizeY, imageSizeX];
            int[,] angleResult = new int[imageSizeY, imageSizeX];
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
                    angleResult[i, j] = direction;
                }
            }

            float[,] nmsResult = new float[imageSizeY, imageSizeX];
            for (int i = 1; i < imageSizeY - 1; i++)
            {
                for (int j = 1; j < imageSizeX - 1; j++)
                {
                    int angle = angleResult[i, j];
                    if (angle == 2 || angle == 6)
                    {
                        if (amplitudeResult[i, j] > amplitudeResult[i - 1, j] && amplitudeResult[i, j] > amplitudeResult[i + 1, j])
                        {
                            nmsResult[i, j] = amplitudeResult[i, j];
                        }
                    }
                    else
                    {
                        if (angle == 1 || angle == 5)
                        {
                            if (amplitudeResult[i, j] > amplitudeResult[i - 1, j + 1] && amplitudeResult[i, j] > amplitudeResult[i + 1, j - 1])
                            {
                                nmsResult[i, j] = amplitudeResult[i, j];
                            }
                        }
                        else
                        {
                            if (angle == 3 || angle == 7)
                            {
                                if (amplitudeResult[i, j] > amplitudeResult[i - 1, j - 1] && amplitudeResult[i, j] > amplitudeResult[i + 1, j + 1])
                                {
                                    nmsResult[i, j] = amplitudeResult[i, j];
                                }
                            }
                            else
                            {
                                if (amplitudeResult[i, j] > amplitudeResult[i, j - 1] && amplitudeResult[i, j] > amplitudeResult[i, j + 1])
                                {
                                    nmsResult[i, j] = amplitudeResult[i, j];
                                }
                            }
                        }
                    }
                }
            }

            // Hysteresis thresolding
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

            byte[,] outputGray = ProcessingImageUtils.truncateToDisplay(hysteresisResult);
            pi.setGray(outputGray);

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

        #endregion


        private struct Position2d
        {
            public int x;
            public int y;
        }
    }
}
