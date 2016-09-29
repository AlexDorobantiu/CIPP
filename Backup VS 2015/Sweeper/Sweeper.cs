using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.MotionRecognition;

namespace Plugins.MotionRecognition.Sweeper
{
    public class Sweeper : IMotionRecognition
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();
        static Sweeper()
        {
            parameters.Add(new ParametersInt32(1, 128, 16, "Block Size:", DisplayType.textBox));
            parameters.Add(new ParametersInt32(1, 128, 16, "Search Distance:", DisplayType.textBox));
            string[] values = { "SAD", "SSD", "Corelatie" };
            parameters.Add(new ParametersEnum("Compare Method:", 1, values, DisplayType.listBox));
            parameters.Add(new ParametersFloat(-1, 1, 0.5f, "Minimum Corelation:", DisplayType.textBox));
        }

        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        int blockSize;
        int searchDistance;
        int compareMethod;
        float minimumCorelation;

        public Sweeper(int blockSize, int searchDistance, int compareMethod, float minimumCorelation)
        {
            this.blockSize = blockSize;
            this.searchDistance = searchDistance;
            this.compareMethod = compareMethod;
            this.minimumCorelation = minimumCorelation;
        }

        #region IMotionRecognition Members

        public MotionVectorBase[,] scan(ProcessingImage frame, ProcessingImage nextFrame)
        {
            MotionVectorBase[,] mv = MotionVectors.getMotionVectorArray(frame, blockSize, searchDistance);

            int frameSizeX = frame.getSizeX();
            int frameSizeY = frame.getSizeY();

            if (nextFrame.getSizeX() != frameSizeX || nextFrame.getSizeY() != frameSizeY) return mv;

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
                                int gasitX = 0;
                                int gasitY = 0;
                                long bestMatch = long.MaxValue;
                                for (int i = -searchDistance; i <= searchDistance; i++)
                                {
                                    for (int j = -searchDistance; j <= searchDistance; j++)
                                    {
                                        long sum = 0;
                                        for (int y = firstY; y < firstY + blockSize; y++)
                                            for (int x = firstX; x < firstX + blockSize; x++)
                                            {
                                                int valoareOriginal = image[y, x];
                                                int valoareCautat = nextImage[y + i, x + j];

                                                if (compareMethod == 0)
                                                    sum += Math.Abs(valoareCautat - valoareOriginal);
                                                else
                                                    sum += ((valoareCautat - valoareOriginal) * (valoareCautat - valoareOriginal));
                                            }
                                        if (sum < bestMatch)
                                        {
                                            bestMatch = sum;
                                            gasitX = j;
                                            gasitY = i;
                                        }
                                    }
                                }
                                mv[blockY, blockX] = new SimpleMotionVector(gasitX, gasitY);
                                blockX++;
                            }
                            blockY++;
                        }
                    } break;
                case 2:
                    {
                        int blockY = 0;
                        for (int firstY = searchDistance; firstY < frameSizeY - searchDistance - blockSize + 1; firstY += blockSize)
                        {
                            int blockX = 0;
                            for (int firstX = searchDistance; firstX < frameSizeX - searchDistance - blockSize + 1; firstX += blockSize)
                            {

                                int templateSize = blockSize * blockSize;
                                int sum = 0;
                                int sqrsum = 0;
                                double prodsum = 0;

                                for (int i = 0; i < blockSize; i++)
                                    for (int j = 0; j < blockSize; j++)
                                    {
                                        int gray = image[firstY + i, firstX + j];
                                        sum += gray;
                                        sqrsum += gray * gray;
                                    }

                                double mean_template = (double)sum / templateSize;
                                double var2template = (double)sqrsum - (((double)sum * sum) / templateSize);

                                double corelatieMaxima = double.MinValue;

                                int gasitX = 0;
                                int gasitY = 0;

                                for (int i = -searchDistance; i <= searchDistance; i++)
                                {
                                    for (int j = -searchDistance; j <= searchDistance; j++)
                                    {
                                        sum = 0;
                                        sqrsum = 0;
                                        prodsum = 0;
                                        for (int y = firstY; y < firstY + blockSize; y++)
                                            for (int x = firstX; x < firstX + blockSize; x++)
                                            {
                                                int cImage = nextImage[y + i, x + j];
                                                int cTemplate = image[y, x];

                                                sum += cImage;
                                                sqrsum += cImage * cImage;
                                                prodsum += cImage * cTemplate;
                                            }
                                        double var2image = (double)sqrsum - (((double)sum * sum) / templateSize);
                                        double radical = Math.Sqrt(var2image * var2template);
                                        if (radical != 0)
                                        {
                                            double coeficientCorelatie = (prodsum - (mean_template * sum)) / radical;
                                            if (coeficientCorelatie > corelatieMaxima)
                                            {
                                                corelatieMaxima = coeficientCorelatie;
                                                gasitY = i;
                                                gasitX = j;
                                            }
                                        }
                                    }                                   
                                }
                                if (corelatieMaxima > minimumCorelation)                                
                                    mv[blockY, blockX] = new SimpleMotionVector(gasitX, gasitY);                                
                                else
                                    mv[blockY, blockX] = new SimpleMotionVector(0, 0);
                                blockX++;
                            }
                            blockY++;
                        }
                    } break;

            }
            return mv;
        }

        #endregion
    }
}
