using System;
using System.Collections.Generic;
using System.Text;
using Plugins.Filters;
using ProcessingImageSDK;
using ParametersSDK;

namespace KMeansFilter
{
    public class KMeansFilter : IFilter
    {

        private static string[] distanceEnumValues = { "SAD", "SSD", "Luminance" };

        public static List<IParameters> getParametersList()
        {
            List<IParameters> parameters = new List<IParameters>();
            parameters.Add(new ParametersInt32(2, int.MaxValue, 16, "Classes:", ParameterDisplayTypeEnum.textBox));
            parameters.Add(new ParametersEnum("Distance function:", 0, distanceEnumValues, ParameterDisplayTypeEnum.listBox));
            return parameters;
        }

        private int classes;
        private int distanceFunction;

        public KMeansFilter(int classes, int distanceFunction)
        {
            this.classes = classes;
            this.distanceFunction = distanceFunction;
        }

        public ImageDependencies getImageDependencies()
        {
            return null;
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage outputImage = new ProcessingImage();
            outputImage.copyAttributesAndAlpha(inputImage);
            outputImage.addWatermark("KMeans Filter, classes: " + classes + " Distance function: " + distanceEnumValues[distanceFunction] + " v1.0, Alex Dorobantiu");

            int sizeX = inputImage.getSizeX();
            int sizeY = inputImage.getSizeY();

            int[,] clusterIndex = new int[sizeY, sizeX];
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    clusterIndex[i, j] = -1;
                }
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

                RgbCluster[] rgbClusters = new RgbCluster[classes];

                // pick the cluster starting points on the main diagonal (not random, deterministic)
                int positionX = 0, positionY = 0;
                int dx = sizeX / classes;
                int dy = sizeY / classes;
                for (int i = 0; i < classes; i++)
                {
                    Rgb rgb = new Rgb(red[positionY, positionX], green[positionY, positionX], blue[positionY, positionX]);
                    rgbClusters[i] = new RgbCluster(i, rgb);
                    positionX += dx;
                    positionY += dy;
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

                GrayCluster[] grayClusters = new GrayCluster[classes];

                // pick the cluster starting points on the main diagonal (not random, deterministic)
                int positionX = 0, positionY = 0;
                int dx = sizeX / classes;
                int dy = sizeY / classes;
                for (int i = 0; i < classes; i++)
                {
                    grayClusters[i] = new GrayCluster(i, gray[positionY, positionX]);
                    positionX += dx;
                    positionY += dy;
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

        static GrayCluster findNearestCluster(GrayCluster[] grayClusters, byte gray, int distanceFunction)
        {
            GrayCluster cluster = null;
            int minDistance = int.MaxValue;
            for (int i = 0; i < grayClusters.Length; i++)
            {
                int distance = computeDistance(grayClusters[i], gray, distanceFunction);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    cluster = grayClusters[i];
                }
            }
            return cluster;
        }

        static int computeDistance(GrayCluster cluster, byte gray, int distanceFunction)
        {
            byte clusterGray = cluster.getGray();
            int distanceGray = clusterGray - gray;
            switch (distanceFunction)
            {
                default: return Math.Abs(distanceGray); // the existing ones behave the same
            }
        }

        static RgbCluster findNearestCluster(RgbCluster[] rgbClusters, Rgb rgb, int distanceFunction)
        {
            RgbCluster cluster = null;
            int minDistance = int.MaxValue;
            for (int i = 0; i < rgbClusters.Length; i++)
            {
                int distance = computeDistance(rgbClusters[i], rgb, distanceFunction);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    cluster = rgbClusters[i];
                }
            }
            return cluster;
        }

        static int computeDistance(RgbCluster cluster, Rgb rgb, int distanceFunction)
        {
            Rgb clusterRgb = cluster.getRgb();
            int distanceR = clusterRgb.r - rgb.r;
            int distanceG = clusterRgb.g - rgb.g;
            int distanceB = clusterRgb.b - rgb.b;
            switch (distanceFunction)
            {
                case 1: return distanceR * distanceR + distanceG * distanceG + distanceB * distanceB;
                case 2: return (int)(Math.Abs(distanceR) * 0.3f + Math.Abs(distanceG) * 0.59f + Math.Abs(distanceB) * 0.11f);
                default: return Math.Abs(distanceR) + Math.Abs(distanceG) + Math.Abs(distanceB);
            }
        }

    }
}
