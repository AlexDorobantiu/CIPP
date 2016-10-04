using System;
using System.Collections.Generic;
using System.Text;
using ParametersSDK;
using Plugins.Filters;
using ProcessingImageSDK;

namespace CropFilter
{
    public class CropFilter : IFilter
    {
        private static readonly List<IParameters> parameters = new List<IParameters>();
        static CropFilter()
        {
            parameters.Add(new ParametersInt32(0, int.MaxValue, 0, "Left:", DisplayType.textBox));
            parameters.Add(new ParametersInt32(0, int.MaxValue, 0, "Rigth:", DisplayType.textBox));
            parameters.Add(new ParametersInt32(0, int.MaxValue, 0, "Top:", DisplayType.textBox));
            parameters.Add(new ParametersInt32(0, int.MaxValue, 0, "Bottom:", DisplayType.textBox));
        }
        public static List<IParameters> getParametersList()
        {
            return parameters;
        }

        private int left, right, top, bottom;
        public CropFilter(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(-1, -1, -1, -1);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            ProcessingImage result = new ProcessingImage();
            result.setName(inputImage.getName());

            int newSizeX = inputImage.getSizeX() - left - right;
            int newSizeY = inputImage.getSizeY() - top - bottom;
            if (newSizeX < 0 || newSizeY < 0)
            {
                return result;
            }

            result.setSizeX(newSizeX);
            result.setSizeY(newSizeY);

            byte[,] alpha = new byte[newSizeY, newSizeX];
            result.setAlpha(alpha);
            for (int i = 0; i < newSizeY; i++)
            {
                for (int j = 0; j < newSizeX; j++)
                {
                    alpha[i, j] = 255;
                }
            }

            if (inputImage.grayscale)
            {
                byte[,] gray = new byte[newSizeY, newSizeX];
                result.grayscale = true;
                result.setGray(gray);

                for (int i = 0; i < newSizeY; i++)
                {
                    for (int j = 0; j < newSizeX; j++)
                    {
                        gray[i, j] = inputImage.getGray()[i + top, j + left];
                    }
                }
            }
            else
            {
                byte[,] red = new byte[newSizeY, newSizeX];
                byte[,] green = new byte[newSizeY, newSizeX];
                byte[,] blue = new byte[newSizeY, newSizeX];
                result.setRed(red);
                result.setGreen(green);
                result.setBlue(blue);

                for (int i = 0; i < newSizeY; i++)
                {
                    for (int j = 0; j < newSizeX; j++)
                    {
                        red[i, j] = inputImage.getRed()[i + top, j + left];
                        green[i, j] = inputImage.getGreen()[i + top, j + left];
                        blue[i, j] = inputImage.getBlue()[i + top, j + left];
                    }
                }
            }
            return result;
        }
    }
}
