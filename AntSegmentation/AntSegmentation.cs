using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace Plugins.Filters.AntSegmentation
{
    public class RandomAnt
    {
        Random random = new Random();
        static int[] addX = { 0, 1, 1, 1, 0, -1, -1, -1 };
        static int[] addY = { -1, -1, 0, 1, 1, 1, 0, -1 };
        private readonly int maxX;
        private readonly int maxY;
        public int positionX;
        public int positionY;
        public int nextX;
        public int nextY;
        public bool plus;

        public RandomAnt(int maxX, int maxY)
        {
            this.maxX = maxX;
            this.maxY = maxY;
            positionX = random.Next(maxX);
            positionY = random.Next(maxY);
            changeNext();
        }

        public void changeNext()
        {
            int direction;

            direction = random.Next(8);
            while (positionX + addX[direction] < 0 || positionX + addX[direction] >= maxX ||
                   positionY + addY[direction] < 0 || positionY + addY[direction] >= maxY)
                direction = random.Next(8);

            nextX = positionX + addX[direction];
            nextY = positionY + addY[direction];
        }

        public void step()
        {
            positionX = nextX;
            positionY = nextY;
            changeNext();
        }
    }

    public class Ant
    {
        Random random = new Random();
        public float positionX;
        public float positionY;
        public float direction;
        public float nextX;
        public float nextY;
        private readonly int maxX;
        private readonly int maxY;
        public bool plus;

        public Ant(int maxX, int maxY)
        {
            this.maxX = maxX;
            this.maxY = maxY;
            positionX = random.Next(maxX);
            positionY = random.Next(maxY);
            changeDirection();
        }

        public void changeDirection()
        {
            direction = (float)(random.NextDouble() * 2 * Math.PI);
            changeNext();

            while (nextX < 0 || nextX >= maxX ||
                   nextY < 0 || nextY >= maxY)
            {
                direction = (float)(random.NextDouble() * 2 * Math.PI);
                changeNext();
            }
        }

        private void changeNext()
        {
            nextX = (float)(positionX + Math.Cos(direction));
            nextY = (float)(positionY + Math.Sin(direction));
            if ((int)nextX == (int)positionX && (int)nextY == (int)positionY)
            {
                nextX = (float)(positionX + Math.Cos(direction));
                nextY = (float)(positionY + Math.Sin(direction));
            }
            if ((nextX < 0 || nextX >= maxX ||
                 nextY < 0 || nextY >= maxY))
                changeDirection();
        }

        public void step()
        {
            positionX = nextX;
            positionY = nextY;
            changeNext();
        }
    }

    public class AntSegmentation : IFilter
    {
        public static List<IParameters> getParametersList()
        {
            List<IParameters> list = new List<IParameters>();
            list.Add(new ParametersInt32(1, 10000, 100, "Ant Number:", DisplayType.textBox));
            list.Add(new ParametersInt32(1, int.MaxValue, 10000, "Number of steps:", DisplayType.textBox));
            list.Add(new ParametersInt32(1, 65535, 2000, "Edge Threshold:", DisplayType.textBox));
            list.Add(new ParametersInt32(1, 255, 32, "Maximum Component Difference:", DisplayType.textBox));
            list.Add(new ParametersFloat(0, 1f, 0.05f, "Escape Proabability:", DisplayType.textBox));
            string[] values0 = { "Random", "Wall Hit" };
            list.Add(new ParametersEnum("Walk Mode:", 1, values0, DisplayType.listBox));
            string[] values1 = { "Copy", "Mean" };
            list.Add(new ParametersEnum("Ant Mode:", 1, values1, DisplayType.comboBox));
            string[] values2 = { "Keep", "Enhance" };
            list.Add(new ParametersEnum("Edge Mode:", 0, values2, DisplayType.listBox));
            return list;
        }

        int antNumber;
        int numberOfSteps;
        int edgeThreshold;
        int maximumDifference;
        float escapeProbability;
        bool randomWalk;
        bool copyOrMean;
        bool enhanceEdge;

        public AntSegmentation(int antNumber, int numberOfSteps, int edgeThreshold, int maximumDifference, float escapeProbability, int randomWalk, int copyOrMean, int enhanceEdge)
        {
            this.antNumber = antNumber;
            this.numberOfSteps = numberOfSteps;
            this.edgeThreshold = edgeThreshold;
            this.maximumDifference = maximumDifference;
            this.escapeProbability = escapeProbability;
            this.randomWalk = randomWalk == 0 ? false : true;
            this.copyOrMean = copyOrMean == 0 ? false : true;
            this.enhanceEdge = enhanceEdge == 0 ? false : true;
        }

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(-1, -1, -1, -1);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage pi = inputImage.Clone();
            pi.addWatermark("ANT Segmentation, v1.0, Alex Dorobantiu");

            int maxX = pi.getSizeX();
            int maxY = pi.getSizeY();

            Random rand = new Random();

            if (!inputImage.grayscale)
            {
                if (!copyOrMean)
                {
                    byte[,] r = pi.getRed();
                    byte[,] g = pi.getGreen();
                    byte[,] b = pi.getBlue();

                    if (randomWalk)
                    {
                        RandomAnt[] ants = new RandomAnt[antNumber];
                        for (int i = 0; i < antNumber; i++) ants[i] = new RandomAnt(maxX, maxY);

                        for (int step = 0; step < numberOfSteps; step++)
                        {
                            for (int i = 0; i < antNumber; i++)
                            {
                                int x = ants[i].positionX;
                                int y = ants[i].positionY;
                                int nx = ants[i].nextX;
                                int ny = ants[i].nextY;
                                int redEdge = r[y, x] - r[ny, nx];
                                int greenEdge = g[y, x] - g[ny, nx];
                                int blueEdge = b[y, x] - b[ny, nx];
                                if (Math.Abs(redEdge) > maximumDifference || Math.Abs(blueEdge) > maximumDifference || Math.Abs(greenEdge) > maximumDifference || redEdge * redEdge + greenEdge * greenEdge + blueEdge * blueEdge > edgeThreshold)
                                {
                                    double probability;
                                    lock (rand)
                                    {
                                        probability = rand.NextDouble();
                                    }
                                    if (probability <= escapeProbability)
                                    {
                                        ants[i].step();
                                    }
                                    else
                                    {
                                        if (enhanceEdge)
                                        {
                                            if ((r[y, x] * 0.3 + g[y, x] * 0.59 + b[y, x] * 0.11) > (r[ny, nx] * 0.3 + g[ny, nx] * 0.59 + b[ny, nx] * 0.11))
                                            {
                                                if (r[ny, nx] > 0) r[ny, nx]--;
                                                if (g[ny, nx] > 0) g[ny, nx]--;
                                                if (b[ny, nx] > 0) b[ny, nx]--;
                                                if (r[y, x] < 255) r[y, x]++;
                                                if (g[y, x] < 255) g[y, x]++;
                                                if (b[y, x] < 255) b[y, x]++;
                                            }
                                            else
                                            {
                                                if (r[ny, nx] < 255) r[ny, nx]++;
                                                if (g[ny, nx] < 255) g[ny, nx]++;
                                                if (b[ny, nx] < 255) b[ny, nx]++;
                                                if (r[y, x] > 0) r[y, x]--;
                                                if (g[y, x] > 0) g[y, x]--;
                                                if (b[y, x] > 0) b[y, x]--;
                                            }
                                        }
                                        ants[i].changeNext();
                                    }
                                }
                                else
                                {
                                    r[ny, nx] = r[y, x];
                                    g[ny, nx] = g[y, x];
                                    b[ny, nx] = b[y, x];
                                    ants[i].step();
                                }
                            }
                        }
                    }
                    else
                    {
                        Ant[] ants = new Ant[antNumber];
                        for (int i = 0; i < antNumber; i++) ants[i] = new Ant(maxX, maxY);
                        for (int step = 0; step < numberOfSteps; step++)
                        {
                            for (int i = 0; i < antNumber; i++)
                            {
                                int x = (int)ants[i].positionX;
                                int y = (int)ants[i].positionY;
                                int nx = (int)ants[i].nextX;
                                int ny = (int)ants[i].nextY;
                                int redEdge = r[y, x] - r[ny, nx];
                                int greenEdge = g[y, x] - g[ny, nx];
                                int blueEdge = b[y, x] - b[ny, nx];
                                if (Math.Abs(redEdge) > maximumDifference || Math.Abs(blueEdge) > maximumDifference || Math.Abs(greenEdge) > maximumDifference || redEdge * redEdge + greenEdge * greenEdge + blueEdge * blueEdge > edgeThreshold)
                                {
                                    double probability;
                                    lock (rand)
                                    {
                                        probability = rand.NextDouble();
                                    }
                                    if (probability <= escapeProbability)
                                    {
                                        ants[i].step();
                                    }
                                    else
                                    {
                                        if (enhanceEdge)
                                        {
                                            if ((r[y, x] * 0.3 + g[y, x] * 0.59 + b[y, x] * 0.11) > (r[ny, nx] * 0.3 + g[ny, nx] * 0.59 + b[ny, nx] * 0.11))
                                            {
                                                if (r[ny, nx] > 0) r[ny, nx]--;
                                                if (g[ny, nx] > 0) g[ny, nx]--;
                                                if (b[ny, nx] > 0) b[ny, nx]--;
                                                if (r[y, x] < 255) r[y, x]++;
                                                if (g[y, x] < 255) g[y, x]++;
                                                if (b[y, x] < 255) b[y, x]++;
                                            }
                                            else
                                            {
                                                if (r[ny, nx] < 255) r[ny, nx]++;
                                                if (g[ny, nx] < 255) g[ny, nx]++;
                                                if (b[ny, nx] < 255) b[ny, nx]++;
                                                if (r[y, x] > 0) r[y, x]--;
                                                if (g[y, x] > 0) g[y, x]--;
                                                if (b[y, x] > 0) b[y, x]--;
                                            }
                                        }
                                        ants[i].changeDirection();
                                    }
                                }
                                else
                                {
                                    r[ny, nx] = r[y, x];
                                    g[ny, nx] = g[y, x];
                                    b[ny, nx] = b[y, x];
                                    ants[i].step();
                                }
                            }
                        }
                    }
                }
                else
                {
                    byte[,] red = pi.getRed();
                    byte[,] green = pi.getGreen();
                    byte[,] blue = pi.getBlue();
                    float[,] r = new float[maxY, maxX];
                    float[,] g = new float[maxY, maxX];
                    float[,] b = new float[maxY, maxX];
                    for (int i = 0; i < maxY; i++)
                        for (int j = 0; j < maxX; j++)
                        {
                            r[i, j] = red[i, j];
                            g[i, j] = green[i, j];
                            b[i, j] = blue[i, j];
                        }
                    if (randomWalk)
                    {
                        RandomAnt[] ants = new RandomAnt[antNumber];
                        for (int i = 0; i < antNumber; i++) ants[i] = new RandomAnt(maxX, maxY);

                        for (int step = 0; step < numberOfSteps; step++)
                        {
                            for (int i = 0; i < antNumber; i++)
                            {
                                int x = ants[i].positionX;
                                int y = ants[i].positionY;
                                int nx = ants[i].nextX;
                                int ny = ants[i].nextY;
                                float redEdge = r[y, x] - r[ny, nx];
                                float greenEdge = g[y, x] - g[ny, nx];
                                float blueEdge = b[y, x] - b[ny, nx];
                                if (Math.Abs(redEdge) > maximumDifference || Math.Abs(blueEdge) > maximumDifference || Math.Abs(greenEdge) > maximumDifference || redEdge * redEdge + greenEdge * greenEdge + blueEdge * blueEdge > edgeThreshold)
                                {
                                    double probability;
                                    lock (rand)
                                    {
                                        probability = rand.NextDouble();
                                    }
                                    if (probability <= escapeProbability)
                                    {
                                        ants[i].step();
                                    }
                                    else
                                    {
                                        if (enhanceEdge)
                                        {
                                            if ((r[y, x] * 0.3 + g[y, x] * 0.59 + b[y, x] * 0.11) > (r[ny, nx] * 0.3 + g[ny, nx] * 0.59 + b[ny, nx] * 0.11))
                                            {
                                                if (r[ny, nx] > 0) r[ny, nx]--;
                                                if (g[ny, nx] > 0) g[ny, nx]--;
                                                if (b[ny, nx] > 0) b[ny, nx]--;
                                                if (r[y, x] < 255) r[y, x]++;
                                                if (g[y, x] < 255) g[y, x]++;
                                                if (b[y, x] < 255) b[y, x]++;
                                            }
                                            else
                                            {
                                                if (r[ny, nx] < 255) r[ny, nx]++;
                                                if (g[ny, nx] < 255) g[ny, nx]++;
                                                if (b[ny, nx] < 255) b[ny, nx]++;
                                                if (r[y, x] > 0) r[y, x]--;
                                                if (g[y, x] > 0) g[y, x]--;
                                                if (b[y, x] > 0) b[y, x]--;
                                            }
                                        }
                                        ants[i].changeNext();
                                    }
                                }
                                else
                                {
                                    r[ny, nx] = (r[ny, nx] + r[y, x]) / 2;
                                    g[ny, nx] = (g[ny, nx] + g[y, x]) / 2;
                                    b[ny, nx] = (b[ny, nx] + b[y, x]) / 2;

                                    ants[i].step();
                                }
                            }
                        }
                    }
                    else
                    {
                        Ant[] ants = new Ant[antNumber];
                        for (int i = 0; i < antNumber; i++) ants[i] = new Ant(maxX, maxY);
                        for (int step = 0; step < numberOfSteps; step++)
                        {
                            for (int i = 0; i < antNumber; i++)
                            {
                                int x = (int)ants[i].positionX;
                                int y = (int)ants[i].positionY;
                                int nx = (int)ants[i].nextX;
                                int ny = (int)ants[i].nextY;
                                float redEdge = r[y, x] - r[ny, nx];
                                float greenEdge = g[y, x] - g[ny, nx];
                                float blueEdge = b[y, x] - b[ny, nx];
                                if (Math.Abs(redEdge) > maximumDifference || Math.Abs(blueEdge) > maximumDifference || Math.Abs(greenEdge) > maximumDifference || redEdge * redEdge + greenEdge * greenEdge + blueEdge * blueEdge > edgeThreshold)
                                {
                                    double probability;
                                    lock (rand)
                                    {
                                        probability = rand.NextDouble();
                                    }
                                    if (probability <= escapeProbability)
                                    {
                                        ants[i].step();
                                    }
                                    else
                                    {
                                        if (enhanceEdge)
                                        {
                                            if ((r[y, x] * 0.3 + g[y, x] * 0.59 + b[y, x] * 0.11) > (r[ny, nx] * 0.3 + g[ny, nx] * 0.59 + b[ny, nx] * 0.11))
                                            {
                                                if (r[ny, nx] > 0) r[ny, nx]--;
                                                if (g[ny, nx] > 0) g[ny, nx]--;
                                                if (b[ny, nx] > 0) b[ny, nx]--;
                                                if (r[y, x] < 255) r[y, x]++;
                                                if (g[y, x] < 255) g[y, x]++;
                                                if (b[y, x] < 255) b[y, x]++;
                                            }
                                            else
                                            {
                                                if (r[ny, nx] < 255) r[ny, nx]++;
                                                if (g[ny, nx] < 255) g[ny, nx]++;
                                                if (b[ny, nx] < 255) b[ny, nx]++;
                                                if (r[y, x] > 0) r[y, x]--;
                                                if (g[y, x] > 0) g[y, x]--;
                                                if (b[y, x] > 0) b[y, x]--;
                                            }
                                        }
                                        ants[i].changeDirection();
                                    }
                                }
                                else
                                {
                                    r[ny, nx] = (r[ny, nx] + r[y, x]) / 2;
                                    g[ny, nx] = (g[ny, nx] + g[y, x]) / 2;
                                    b[ny, nx] = (b[ny, nx] + b[y, x]) / 2;
                                    ants[i].step();
                                }
                            }
                        }
                    }
                    for (int i = 0; i < maxY; i++)
                        for (int j = 0; j < maxX; j++)
                        {
                            red[i, j] = (byte)r[i, j];
                            green[i, j] = (byte)g[i, j];
                            blue[i, j] = (byte)b[i, j];
                        }
                }
            }
            else
            {
                if (!copyOrMean)
                {
                    byte[,] gray = pi.getGray();

                    if (randomWalk)
                    {
                        RandomAnt[] ants = new RandomAnt[antNumber];
                        for (int i = 0; i < antNumber; i++) ants[i] = new RandomAnt(maxX, maxY);

                        for (int step = 0; step < numberOfSteps; step++)
                        {
                            for (int i = 0; i < antNumber; i++)
                            {
                                int x = ants[i].positionX;
                                int y = ants[i].positionY;
                                int nx = ants[i].nextX;
                                int ny = ants[i].nextY;
                                int edge = gray[y, x] - gray[ny, nx];
                                if (Math.Abs(edge) > maximumDifference || edge * edge * 3 > edgeThreshold)
                                {
                                    double probability;
                                    lock (rand)
                                    {
                                        probability = rand.NextDouble();
                                    }
                                    if (probability <= escapeProbability)
                                    {
                                        ants[i].step();
                                    }
                                    else
                                    {
                                        if (enhanceEdge)
                                        {
                                            if (gray[y, x] > gray[ny, nx])
                                            {
                                                if (gray[ny, nx] > 0) gray[ny, nx]--;
                                                if (gray[y, x] < 255) gray[y, x]++;
                                            }
                                            else
                                            {
                                                if (gray[ny, nx] < 255) gray[ny, nx]++;
                                                if (gray[y, x] > 0) gray[y, x]--;
                                            }
                                        }
                                        ants[i].changeNext();
                                    }
                                }
                                else
                                {
                                    gray[ny, nx] = gray[y, x];
                                    ants[i].step();
                                }
                            }
                        }
                    }
                    else
                    {
                        Ant[] ants = new Ant[antNumber];
                        for (int i = 0; i < antNumber; i++) ants[i] = new Ant(maxX, maxY);

                        for (int step = 0; step < numberOfSteps; step++)
                        {
                            for (int i = 0; i < antNumber; i++)
                            {
                                int x = (int)ants[i].positionX;
                                int y = (int)ants[i].positionY;
                                int nx = (int)ants[i].nextX;
                                int ny = (int)ants[i].nextY;
                                int edge = gray[y, x] - gray[ny, nx];
                                if (Math.Abs(edge) > maximumDifference || edge * edge * 3 > edgeThreshold)
                                {
                                    double probability;
                                    lock (rand)
                                    {
                                        probability = rand.NextDouble();
                                    }
                                    if (probability <= escapeProbability)
                                    {
                                        ants[i].step();
                                    }
                                    else
                                    {
                                        if (enhanceEdge)
                                        {
                                            if (gray[y, x] > gray[ny, nx])
                                            {
                                                if (gray[ny, nx] > 0) gray[ny, nx]--;
                                                if (gray[y, x] < 255) gray[y, x]++;
                                            }
                                            else
                                            {
                                                if (gray[ny, nx] < 255) gray[ny, nx]++;
                                                if (gray[y, x] > 0) gray[y, x]--;
                                            }
                                        }
                                        ants[i].changeDirection();
                                    }
                                }
                                else
                                {
                                    gray[ny, nx] = gray[y, x];
                                    ants[i].step();
                                }
                            }
                        }
                    }
                }
                else
                {
                    byte[,] g = pi.getGray();
                    float[,] gray = new float[maxY, maxX];
                    for (int i = 0; i < maxY; i++)
                        for (int j = 0; j < maxX; j++)
                            gray[i, j] = g[i, j];
                    if (randomWalk)
                    {
                        RandomAnt[] ants = new RandomAnt[antNumber];
                        for (int i = 0; i < antNumber; i++) ants[i] = new RandomAnt(maxX, maxY);

                        for (int step = 0; step < numberOfSteps; step++)
                        {
                            for (int i = 0; i < antNumber; i++)
                            {
                                int x = ants[i].positionX;
                                int y = ants[i].positionY;
                                int nx = ants[i].nextX;
                                int ny = ants[i].nextY;
                                float edge = gray[y, x] - gray[ny, nx];
                                if (Math.Abs(edge) > maximumDifference || edge * edge * 3 > edgeThreshold)
                                {
                                    double probability;
                                    lock (rand)
                                    {
                                        probability = rand.NextDouble();
                                    }
                                    if (probability <= escapeProbability)
                                    {
                                        ants[i].step();
                                    }
                                    else
                                    {
                                        if (enhanceEdge)
                                        {
                                            if (gray[y, x] > gray[ny, nx])
                                            {
                                                if (gray[ny, nx] > 0) gray[ny, nx]--;
                                                if (gray[y, x] < 255) gray[y, x]++;
                                            }
                                            else
                                            {
                                                if (gray[ny, nx] < 255) gray[ny, nx]++;
                                                if (gray[y, x] > 0) gray[y, x]--;
                                            }
                                        }
                                        ants[i].changeNext();
                                    }
                                }
                                else
                                {
                                    gray[ny, nx] = (gray[ny, nx] + gray[y, x]) / 2;

                                    ants[i].step();
                                }
                            }
                        }
                    }
                    else
                    {
                        Ant[] ants = new Ant[antNumber];
                        for (int i = 0; i < antNumber; i++) ants[i] = new Ant(maxX, maxY);

                        for (int step = 0; step < numberOfSteps; step++)
                        {
                            for (int i = 0; i < antNumber; i++)
                            {
                                int x = (int)ants[i].positionX;
                                int y = (int)ants[i].positionY;
                                int nx = (int)ants[i].nextX;
                                int ny = (int)ants[i].nextY;
                                float edge = gray[y, x] - gray[ny, nx];
                                if (Math.Abs(edge) > maximumDifference || edge * edge * 3 > edgeThreshold)
                                {
                                    double probability;
                                    lock (rand)
                                    {
                                        probability = rand.NextDouble();
                                    }
                                    if (probability <= escapeProbability)
                                    {
                                        ants[i].step();
                                    }
                                    else
                                    {
                                        if (enhanceEdge)
                                        {
                                            if (gray[y, x] > gray[ny, nx])
                                            {
                                                if (gray[ny, nx] > 0) gray[ny, nx]--;
                                                if (gray[y, x] < 255) gray[y, x]++;
                                            }
                                            else
                                            {
                                                if (gray[ny, nx] < 255) gray[ny, nx]++;
                                                if (gray[y, x] > 0) gray[y, x]--;
                                            }
                                        }
                                        ants[i].changeDirection();
                                    }
                                }
                                else
                                {
                                    gray[ny, nx] = (gray[ny, nx] + gray[y, x]) / 2;
                                    ants[i].step();
                                }
                            }
                        }
                    }
                    for (int i = 0; i < maxY; i++)
                        for (int j = 0; j < maxX; j++)
                            g[i, j] = (byte)gray[i, j];
                }
            }
            return pi;
        }

        #endregion
    }
}