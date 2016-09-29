using System;
using System.Collections.Generic;
using System.Text;

using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;

namespace InpaintingFilter
{
    struct Position
    {
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int x;
        public int y;
    }

    public class InpaintingFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();
        static InpaintingFilter()
        {
            parameters.Add(new ParametersInt32(1, 20, 5, "Strength:", DisplayType.trackBar));
            string[] values0 = { "Gradient", "Difussion" };
            parameters.Add(new ParametersEnum("Mode:", 1, values0, DisplayType.listBox));
        }
        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int strength;
        private int type;

        public InpaintingFilter(int strength, int type)
        {
            this.strength = strength;
            this.type = type;
        }

        private byte[,] alpha;
        private int sizeX;
        private int sizeY;

        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(-1, -1, -1, -1);
        }

        private bool exists(int x, int y)
        {
            return (x >= 0 && x < sizeX && y >= 0 && y <= sizeY);
        }

        private int length(Position p)
        {
            return (p.x * p.x + p.y * p.y);
        }

        private int[,] sobelX = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        private int[,] sobelY = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
        
        private Position gradient(byte[,] channel, int x, int y)
        {
            int sumX = 0;
            int sumY = 0;
            for (int i = y - 1; i <= y + 1; i++)
                for (int j = x - 1; j <= x + 1; j++)
                    if (exists(j, i) && alpha[i, j] == 255)
                    {
                        sumX += sobelX[i - y + 1, j - x + 1] * channel[i, j];
                        sumY += sobelY[i - y + 1, j - x + 1] * channel[i, j];
                    }
            return new Position(sumX, sumY);
        }

        private int dotProduct(Position p1, Position p2)
        {
            return p1.x * p2.x + p1.y + p2.y;
        }

        private void inpaintGradient(byte[,] channel, int x, int y)
        {
            int startX = x - strength;
            int startY = y - strength;
            int endX = x + strength;
            int endY = y + strength;

            if (startX < 0) startX = 0;
            if (startY < 0) startY = 0;
            if (endX >= sizeX) endX = sizeX - 1;
            if (endY >= sizeY) endY = sizeY - 1;

            double intensity = 0;
            double sum = 0;
            for (int i = startY; i < endY; i++)
            {
                for (int j = startX; j < endX; j++)
                    if (alpha[i, j] == 255)
                    {
                        Position r = new Position(x - j, y - i);
                        Position grad = gradient(channel, j, i);
                        double dir = (double)dotProduct(r, grad) / length(r);
                        double dst = 1.0 / (length(r) * length(r));
                        //double lev = 1.0 / (1 + Math.Abs(channel[i, j] - channel[y, x]));
                        double lev = 1.0 / (1 + Math.Abs(dotProduct(gradient(channel, j, i), gradient(channel, x, y))));

                        double w = Math.Abs(dir * dst * lev);
                        Position g = new Position();
                        if (exists(j + 1, i) && exists(j - 1, i) && alpha[i, j + 1] == 255 && alpha[i, j - 1] == 255)
                            g.x = channel[i, j + 1] - channel[i, j - 1];
                        if (exists(j, i + 1) && exists(j, i - 1) && alpha[i + 1, j] == 255 && alpha[i - 1, j] == 255)
                            g.y = channel[i + 1, j] - channel[i - 1, j];

                        intensity += w * (channel[i, j] + dotProduct(g, r));
                        sum += w;
                    }
            }
            if (sum != 0)
            {
                int temp = (int)(intensity / sum);
                if (temp < 0) temp = 0;
                if (temp > 255) temp = 255;
                channel[y, x] = (byte)temp;
            }
        }


        private double[,] inpaintMatrix = new double[,] { { 0.073235, 0.176765, 0.073235 }, 
                                                          { 0.176765, 0, 0.176765 }, 
                                                          { 0.073235, 0.176765, 0.073235 } };
        private void inpaintDifussion(byte[,] channel, int x, int y)
        {
            int count = 0;
            double sum = 0;
            for (int i = y - 1; i <= y + 1; i++)
                for (int j = x - 1; j <= x + 1; j++)
                    if (exists(j, i) && alpha[i, j] == 255)
                    {
                        sum += inpaintMatrix[i - y + 1, j - x + 1] * channel[i, j];
                        count++;
                    }
            sum = sum * 8 / count;
            if (sum < 0) sum = 0;
            if (sum > 255) sum = 255;
            channel[y, x] = (byte)sum;
        }

        private void prepareBorder(List<Position> border)
        {
            #region prepare border
            for (int i = 1; i < sizeY - 1; i++)
                for (int j = 1; j < sizeX - 1; j++)
                {
                    if (alpha[i, j] != 255)
                        if (alpha[i - 1, j - 1] == 255 || alpha[i + 1, j - 1] == 255 ||
                            alpha[i - 1, j + 1] == 255 || alpha[i + 1, j + 1] == 255 ||
                            alpha[i, j + 1] == 255 || alpha[i, j - 1] == 255 ||
                            alpha[i - 1, j] == 255 || alpha[i + 1, j] == 255)
                            border.Add(new Position(j, i));
                }
            for (int i = 1; i < sizeY - 1; i++)
            {
                if (alpha[i, 0] != 255)
                    if (alpha[i - 1, 0] == 255 || alpha[i + 1, 0] == 255 ||
                        alpha[i - 1, 1] == 255 || alpha[i + 1, 1] == 255 ||
                        alpha[i, 1] == 255)
                        border.Add(new Position(0, i));
                if (alpha[i, sizeX - 1] != 255)
                    if (alpha[i - 1, sizeX - 1] == 255 || alpha[i + 1, sizeX - 1] == 255 ||
                        alpha[i, sizeX - 2] == 255 ||
                        alpha[i - 1, sizeX - 2] == 255 || alpha[i + 1, sizeX - 2] == 255)
                        border.Add(new Position(sizeX - 1, i));
            }
            for (int j = 1; j < sizeX - 1; j++)
            {
                if (alpha[0, j] != 255)
                    if (alpha[0, j - 1] == 255 || alpha[0, j + 1] == 255 ||
                        alpha[1, j + 1] == 255 || alpha[1, j - 1] == 255 ||
                        alpha[1, j] == 255)
                        border.Add(new Position(j, 0));
                if (alpha[sizeY - 1, j] != 255)
                    if (alpha[sizeY - 1, j - 1] == 255 || alpha[sizeY - 1, j + 1] == 255 ||
                        alpha[sizeY - 2, j + 1] == 255 || alpha[sizeY - 2, j - 1] == 255 ||
                        alpha[sizeY - 2, j] == 255)
                        border.Add(new Position(j, sizeY - 1));
            }

            if (alpha[0, 0] != 255)
            {
                if (alpha[0, 1] == 255 || alpha[1, 0] == 255 || alpha[1, 1] == 255)
                    border.Add(new Position(0, 0));
            }

            if (alpha[0, sizeX - 1] != 255)
            {
                if (alpha[0, sizeX - 2] == 255 || alpha[1, sizeX - 1] == 255 || alpha[1, sizeX - 2] == 255)
                    border.Add(new Position(0, sizeX - 1));
            }

            if (alpha[sizeY - 1, 0] != 255)
            {
                if (alpha[sizeY - 1, 1] == 255 || alpha[sizeY - 2, 0] == 255 || alpha[sizeY - 2, 1] == 255)
                    border.Add(new Position(sizeY - 1, 0));
            }

            if (alpha[sizeY - 1, sizeX - 1] != 255)
            {
                if (alpha[sizeY - 1, sizeX - 2] == 255 ||
                    alpha[sizeY - 2, sizeX - 2] == 255 ||
                    alpha[sizeY - 2, sizeX - 1] == 255)
                    border.Add(new Position(sizeY - 1, sizeX - 1));
            }
            #endregion
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            sizeX = inputImage.getSizeX();
            sizeY = inputImage.getSizeY();


            byte[,] red = (byte[,])inputImage.getRed().Clone();
            byte[,] green = (byte[,])inputImage.getGreen().Clone();
            byte[,] blue = (byte[,])inputImage.getBlue().Clone();

            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributes(inputImage);


            List<Position> border = new List<Position>();

            if (type == 1)
            {
                for (int iteration = 0; iteration < strength; iteration++)
                {
                    alpha = (byte[,])inputImage.getAlpha().Clone();
                    do
                    {
                        border.Clear();

                        prepareBorder(border);

                        foreach (Position p in border)
                        {
                            inpaintDifussion(red, p.x, p.y);
                            inpaintDifussion(green, p.x, p.y);
                            inpaintDifussion(blue, p.x, p.y);

                            alpha[p.y, p.x] = 255;
                        }
                    }
                    while (border.Count != 0);
                }
            }
            else
            {
                alpha = (byte[,])inputImage.getAlpha().Clone();
                do
                {
                    border.Clear();

                    prepareBorder(border);

                    foreach (Position p in border)
                    {
                        inpaintGradient(red, p.x, p.y);
                        inpaintGradient(green, p.x, p.y);
                        inpaintGradient(blue, p.x, p.y);

                        alpha[p.y, p.x] = 255;
                    }
                }
                while (border.Count != 0);
            }

            pi.setRed(red);
            pi.setGreen(green);
            pi.setBlue(blue);
            pi.setAlpha(alpha);
            pi.addWatermark("Inpaint Filter (" + (type == 1 ? "difussion" : "gradient") + "), strength: " + strength + " v1.0, Alex Dorobantiu");
            return pi;
        }

        #endregion
    }
}
