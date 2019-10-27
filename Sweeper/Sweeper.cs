using System;
using System.Collections.Generic;

using ProcessingImageSDK;
using ParametersSDK;
using ProcessingImageSDK.MotionVectors;

namespace Plugins.MotionRecognition.Sweeper
{
    public class Sweeper : IMotionRecognition
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();

        static Sweeper()
        {
            parameters.Add(new ParametersInt32(1, 128, 16, "Block Size:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersInt32(1, 128, 16, "Search Distance:", ParameterDisplayTypeEnum.textBox));
            string[] compareMethodValues = { "SumOfAbsoluteDifferences", "SumOfSquareDistances", "Correlation" };
            parameters.Add(new ParametersEnum("Compare Method:", 1, compareMethodValues, ParameterDisplayTypeEnum.listBox));
            parameters.Add(new ParametersFloat(-1, 1, 0.5f, "Minimum Correlation:", ParameterDisplayTypeEnum.textBox));
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        int blockSize;
        int searchDistance;
        int compareMethod;
        float minimumCorrelation;

        public Sweeper(int blockSize, int searchDistance, int compareMethod, float minimumCorrelation)
        {
            this.blockSize = blockSize;
            this.searchDistance = searchDistance;
            this.compareMethod = compareMethod;
            this.minimumCorrelation = minimumCorrelation;
        }

        #region IMotionRecognition Members

        public MotionVectorBase[,] scan(ProcessingImage frame, ProcessingImage nextFrame)
        {
            MotionVectorBase[,] motionVectors = MotionVectorUtils.getMotionVectorArray(frame, blockSize, searchDistance);

            int frameSizeX = frame.getSizeX();
            int frameSizeY = frame.getSizeY();

            if (nextFrame.getSizeX() != frameSizeX || nextFrame.getSizeY() != frameSizeY)
            {
                return motionVectors;
            }

            byte[,] image = frame.getGray();
            byte[,] nextImage = nextFrame.getGray();

            switch (compareMethod)
            {
                case 0:
                case 1:
                    {
                        int blockY = 0;
                        for (int firstY = searchDistance; firstY < frameSizeY - searchDistance - blockSize + 1; firstY += blockSize)
                        {
                            int blockX = 0;
                            for (int firstX = searchDistance; firstX < frameSizeX - searchDistance - blockSize + 1; firstX += blockSize)
                            {
                                int foundX = 0;
                                int foundY = 0;
                                long bestMatch = long.MaxValue;
                                for (int i = -searchDistance; i <= searchDistance; i++)
                                {
                                    for (int j = -searchDistance; j <= searchDistance; j++)
                                    {
                                        long sum = 0;
                                        for (int y = firstY; y < firstY + blockSize; y++)
                                        {
                                            for (int x = firstX; x < firstX + blockSize; x++)
                                            {
                                                int originalValue = image[y, x];
                                                int searchValue = nextImage[y + i, x + j];

                                                int difference = searchValue - originalValue;
                                                if (compareMethod == 0)
                                                {
                                                    sum += Math.Abs(difference);
                                                }
                                                else
                                                {
                                                    sum += (difference * difference);
                                                }
                                            }
                                        }
                                        if (sum < bestMatch)
                                        {
                                            bestMatch = sum;
                                            foundX = j;
                                            foundY = i;
                                        }
                                    }
                                }
                                motionVectors[blockY, blockX] = new SimpleMotionVector(foundX, foundY);
                                blockX++;
                            }
                            blockY++;
                        }
                    } break;
                case 2:
                    {
                        int blockArea = blockSize * blockSize;
                        int blockY = 0;
                        for (int firstY = searchDistance; firstY < frameSizeY - searchDistance - blockSize + 1; firstY += blockSize)
                        {
                            int blockX = 0;
                            for (int firstX = searchDistance; firstX < frameSizeX - searchDistance - blockSize + 1; firstX += blockSize)
                            {
                                int sum = 0;
                                int squaredSum = 0;
                                double productSum = 0;

                                for (int i = 0; i < blockSize; i++)
                                {
                                    for (int j = 0; j < blockSize; j++)
                                    {
                                        int gray = image[firstY + i, firstX + j];
                                        sum += gray;
                                        squaredSum += gray * gray;
                                    }
                                }

                                double meanOfOriginal = (double)sum / blockArea;
                                double varianceOfOriginal = (double)squaredSum - (((double)sum * sum) / blockArea);

                                double maximumCorrelation = double.MinValue;

                                int foundX = 0;
                                int foundY = 0;
                                for (int i = -searchDistance; i <= searchDistance; i++)
                                {
                                    for (int j = -searchDistance; j <= searchDistance; j++)
                                    {
                                        sum = 0;
                                        squaredSum = 0;
                                        productSum = 0;
                                        for (int y = firstY; y < firstY + blockSize; y++)
                                        {
                                            for (int x = firstX; x < firstX + blockSize; x++)
                                            {
                                                int originalValue = image[y, x];
                                                int searchValue = nextImage[y + i, x + j];

                                                sum += searchValue;
                                                squaredSum += searchValue * searchValue;
                                                productSum += searchValue * originalValue;
                                            }
                                        }
                                        double varianceOfSearch = (double)squaredSum - (((double)sum * sum) / blockArea);
                                        double squareRootOfVariancesProduct = Math.Sqrt(varianceOfSearch * varianceOfOriginal);
                                        if (squareRootOfVariancesProduct != 0)
                                        {
                                            double correlationCoeficient = (productSum - (meanOfOriginal * sum)) / squareRootOfVariancesProduct;
                                            if (correlationCoeficient > maximumCorrelation)
                                            {
                                                maximumCorrelation = correlationCoeficient;
                                                foundY = i;
                                                foundX = j;
                                            }
                                        }
                                    }
                                }
                                if (maximumCorrelation > minimumCorrelation)
                                {
                                    motionVectors[blockY, blockX] = new SimpleMotionVector(foundX, foundY);
                                }
                                else
                                {
                                    motionVectors[blockY, blockX] = new SimpleMotionVector(0, 0);
                                }
                                blockX++;
                            }
                            blockY++;
                        }
                    } break;

            }
            return motionVectors;
        }

        #endregion
    }
}
