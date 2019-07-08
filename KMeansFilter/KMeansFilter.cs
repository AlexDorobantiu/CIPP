using System;
using System.Collections.Generic;
using System.Text;
using Plugins.Filters;
using ProcessingImageSDK;
using ParametersSDK;
using ProcessingImageSDK.Position;

namespace KMeansFilter
{
    public class KMeansFilter : IFilter
    {

        private static string[] distanceEnumValues = { "SAD", "SSD", "Luminance" };
        private static string[] initialSeedFunctionValues = { "Diagonal", "Random", "KMeans++" };

        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>();
            parameters.Add(new ParametersInt32(2, int.MaxValue, 16, "Number of classes:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersEnum("Distance function:", 0, distanceEnumValues, ParameterDisplayTypeEnum.listBox));
            parameters.Add(new ParametersEnum("Seed Selection:", 0, initialSeedFunctionValues, ParameterDisplayTypeEnum.listBox));
            return parameters;
        }

        private int numberOfClasses;
        private int distanceFunction;
        private int initialSeedFunction;

        public KMeansFilter(int numberOfClasses, int distanceFunction, int initialSeedFunction)
        {
            this.numberOfClasses = numberOfClasses;
            this.distanceFunction = distanceFunction;
            this.initialSeedFunction = initialSeedFunction;
        }

        public ImageDependencies getImageDependencies()
        {
            return null;
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark("KMeans Filter, Number of classes: " + numberOfClasses + ", Distance function: " + distanceEnumValues[distanceFunction] +
                ", Seed selection: " + initialSeedFunctionValues[initialSeedFunction] + " v1.0, Alex Dorobantiu");

            int sizeX = inputImage.getSizeX();
            int sizeY = inputImage.getSizeY();

            // initialize the cluster assignments to "no cluster"
            int[,] clusterIndex = new int[sizeY, sizeX];
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    clusterIndex[i, j] = -1;
                }
            }

            ICollection<Position2d> clusterStartingPoints = getStartingPointsPositions(inputImage, distanceFunction);
            if (clusterStartingPoints.Count != numberOfClasses)
            {
                throw new Exception("Invalid number of starting points.");
            }

            if (!inputImage.grayscale)
            {
                byte[,] outputRed = new byte[sizeY, sizeX];
                byte[,] outputGreen = new byte[sizeY, sizeX];
                byte[,] outputBlue = new byte[sizeY, sizeX];
                outputImage.setRed(outputRed);
                outputImage.setGreen(outputGreen);
                outputImage.setBlue(outputBlue);

                byte[,] red = inputImage.getRed();
                byte[,] green = inputImage.getGreen();
                byte[,] blue = inputImage.getBlue();

                RgbCluster[] rgbClusters = new RgbCluster[numberOfClasses];
                int positionIndex = 0;
                foreach (Position2d position in clusterStartingPoints)
                {
                    Rgb rgb = new Rgb(red[position.y, position.x], green[position.y, position.x], blue[position.y, position.x]);
                    rgbClusters[positionIndex] = new RgbCluster(positionIndex, rgb);
                    positionIndex += 1;
                }

                bool clusterChanged = true;
                while (clusterChanged)
                {
                    clusterChanged = false;
                    for (int i = 0; i < sizeY; i++)
                    {
                        for (int j = 0; j < sizeX; j++)
                        {
                            Rgb rgb = new Rgb(red[i, j], green[i, j], blue[i, j]);
                            RgbCluster cluster = findNearestCluster(rgbClusters, rgb, distanceFunction);
                            if (clusterIndex[i, j] != cluster.getIndex())
                            {
                                if (clusterIndex[i, j] != -1)
                                {
                                    rgbClusters[clusterIndex[i, j]].removePixel(rgb);
                                }
                                cluster.addPixel(rgb);
                                clusterIndex[i, j] = cluster.getIndex();
                                clusterChanged = true;
                            }
                        }
                    }
                }

                for (int i = 0; i < sizeY; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        Rgb rgb = rgbClusters[clusterIndex[i, j]].getRgb();
                        outputRed[i, j] = rgb.r;
                        outputGreen[i, j] = rgb.g;
                        outputBlue[i, j] = rgb.b;
                    }
                }
            }
            else
            {
                byte[,] outputGray = new byte[sizeY, sizeX];
                outputImage.setGray(outputGray);

                byte[,] gray = inputImage.getGray();

                GrayCluster[] grayClusters = new GrayCluster[numberOfClasses];
                int positionIndex = 0;
                foreach (Position2d position in clusterStartingPoints)
                {
                    grayClusters[positionIndex] = new GrayCluster(positionIndex, gray[position.y, position.x]);
                    positionIndex += 1;
                }

                bool clusterChanged = true;
                while (clusterChanged)
                {
                    clusterChanged = false;
                    for (int i = 0; i < sizeY; i++)
                    {
                        for (int j = 0; j < sizeX; j++)
                        {
                            GrayCluster cluster = findNearestCluster(grayClusters, gray[i, j], distanceFunction);
                            if (clusterIndex[i, j] != cluster.getIndex())
                            {
                                if (clusterIndex[i, j] != -1)
                                {
                                    grayClusters[clusterIndex[i, j]].removePixel(gray[i, j]);
                                }
                                cluster.addPixel(gray[i, j]);
                                clusterIndex[i, j] = cluster.getIndex();
                                clusterChanged = true;
                            }
                        }
                    }
                }

                for (int i = 0; i < sizeY; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        outputGray[i, j] = grayClusters[clusterIndex[i, j]].getGray();
                    }
                }
            }

            return outputImage;
        }

        private ICollection<Position2d> getStartingPointsPositions(ProcessingImage inputImage, int distanceFunction)
        {
            int sizeX = inputImage.getSizeX();
            int sizeY = inputImage.getSizeY();
            switch (initialSeedFunction)
            {
                case 0:
                    // pick the cluster starting points on the main diagonal (not random, deterministic)
                    ICollection<Position2d> clusterStartingPoints = new List<Position2d>();
                    if (numberOfClasses == 1)
                    {
                        clusterStartingPoints.Add(new Position2d());
                        return clusterStartingPoints;
                    }
                    Position2d position = new Position2d();
                    int dx = (sizeX - 1) / (numberOfClasses - 1);
                    int dy = (sizeY - 1) / (numberOfClasses - 1);
                    for (int i = 0; i < numberOfClasses; i++)
                    {
                        clusterStartingPoints.Add(position);
                        position.x += dx;
                        position.y += dy;
                    }
                    return clusterStartingPoints;
                case 1:
                    // pic cluster starting points randomly but with a fixed seed
                    Random random = new Random(Seed: 123);
                    clusterStartingPoints = new List<Position2d>(); // set is not implemented in .NET 2 (only from 3.5)
                    while (clusterStartingPoints.Count != numberOfClasses)
                    {
                        position = new Position2d(random.Next(sizeX), random.Next(sizeY));
                        if (!clusterStartingPoints.Contains(position))
                        {
                            clusterStartingPoints.Add(position);
                        }
                    }
                    return clusterStartingPoints;
                case 2:
                    // K-Means++
                    // 1. Choose one center uniformly at random from among the data points.
                    // 2. For each data point x, compute D(x), the distance between x and the nearest center that has already been chosen.
                    // 3. Choose one new data point at random as a new center, using a weighted probability distribution where a point x is chosen with probability proportional to D(x)^2.
                    // 4. Repeat Steps 2 and 3 until k centers have been chosen.

                    random = new Random(Seed: 123);
                    clusterStartingPoints = new List<Position2d>();
                    clusterStartingPoints.Add(new Position2d(random.Next(sizeX), random.Next(sizeY))); // step 1

                    double[,] distance = new double[sizeY, sizeX];

                StartStep4Label:
                    // step 4
                    while (clusterStartingPoints.Count != numberOfClasses)
                    {
                        // step 2
                        if (inputImage.grayscale)
                        {
                            byte[,] gray = inputImage.getGray();
                            List<byte> centers = new List<byte>(clusterStartingPoints.Count);
                            foreach (Position2d centerPosition in clusterStartingPoints)
                            {
                                centers.Add(gray[centerPosition.y, centerPosition.x]);
                            }

                            for (int y = 0; y < sizeY; y++)
                            {
                                for (int x = 0; x < sizeX; x++)
                                {
                                    float minDistance = float.MaxValue;
                                    foreach (byte center in centers)
                                    {
                                        minDistance = Math.Min(minDistance, computeDistance(center, gray[y, x], distanceFunction));
                                    }
                                    distance[y, x] = minDistance;
                                }
                            }
                        }
                        else
                        {
                            byte[,] red = inputImage.getRed();
                            byte[,] green = inputImage.getGreen();
                            byte[,] blue = inputImage.getBlue();
                            List<Rgb> centers = new List<Rgb>(clusterStartingPoints.Count);
                            foreach (Position2d centerPosition in clusterStartingPoints)
                            {
                                Rgb rgb = new Rgb(red[centerPosition.y, centerPosition.x], green[centerPosition.y, centerPosition.x], blue[centerPosition.y, centerPosition.x]);
                                centers.Add(rgb);
                            }

                            for (int y = 0; y < sizeY; y++)
                            {
                                for (int x = 0; x < sizeX; x++)
                                {
                                    float minDistance = float.MaxValue;
                                    foreach (Rgb center in centers)
                                    {
                                        Rgb rgb = new Rgb(red[y, x], green[y, x], blue[y, x]);
                                        minDistance = Math.Min(minDistance, computeDistance(center, rgb, distanceFunction));
                                    }
                                    distance[y, x] = minDistance;
                                }
                            }
                        }

                        // step 3
                        double distanceSum = 0;
                        // square the distance
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int x = 0; x < sizeX; x++)
                            {
                                distance[y, x] = distance[y, x] * distance[y, x];
                                distanceSum += distance[y, x];
                            }
                        }
                        // normalize in order to choose probabilistically
                        if (distanceSum != 0)
                        {
                            for (int y = 0; y < sizeY; y++)
                            {
                                for (int x = 0; x < sizeX; x++)
                                {
                                    distance[y, x] /= distanceSum;
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("More classes than different colors in the image");
                        }

                        double sumSoFar = 0;
                        double probability = random.NextDouble();
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int x = 0; x < sizeX; x++)
                            {
                                sumSoFar += distance[y, x];
                                if (sumSoFar > probability)
                                {
                                    clusterStartingPoints.Add(new Position2d(x, y));
                                    goto StartStep4Label;  // break out of 2 loops (and a while) with a goto
                                }
                            }
                        }
                    }

                    return clusterStartingPoints;
                default:
                    throw new NotImplementedException();
            }
        }

        static GrayCluster findNearestCluster(GrayCluster[] grayClusters, byte gray, int distanceFunction)
        {
            GrayCluster cluster = null;
            int minDistance = int.MaxValue;
            for (int i = 0; i < grayClusters.Length; i++)
            {
                int distance = computeDistance(grayClusters[i].getGray(), gray, distanceFunction);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    cluster = grayClusters[i];
                }
            }
            return cluster;
        }

        static int computeDistance(byte clusterGray, byte gray, int distanceFunction)
        {
            int distanceGray = (int)clusterGray - gray;
            switch (distanceFunction)
            {
                default: return Math.Abs(distanceGray); // the existing ones behave the same
            }
        }

        static RgbCluster findNearestCluster(RgbCluster[] rgbClusters, Rgb rgb, int distanceFunction)
        {
            RgbCluster cluster = null;
            float minDistance = int.MaxValue;
            for (int i = 0; i < rgbClusters.Length; i++)
            {
                float distance = computeDistance(rgbClusters[i].getRgb(), rgb, distanceFunction);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    cluster = rgbClusters[i];
                }
            }
            return cluster;
        }

        static float computeDistance(Rgb clusterRgb, Rgb rgb, int distanceFunction)
        {
            int distanceR = clusterRgb.r - rgb.r;
            int distanceG = clusterRgb.g - rgb.g;
            int distanceB = clusterRgb.b - rgb.b;
            switch (distanceFunction)
            {
                case 1: return distanceR * distanceR + distanceG * distanceG + distanceB * distanceB;
                case 2: return Math.Abs(distanceR) * 0.3f + Math.Abs(distanceG) * 0.59f + Math.Abs(distanceB) * 0.11f;
                default: return Math.Abs(distanceR) + Math.Abs(distanceG) + Math.Abs(distanceB);
            }
        }

    }
}
