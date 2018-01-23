using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.IO;
using ProcessingImageSDK.PixelStructures;

namespace ProcessingImageSDK
{
    /// <summary>
    /// ProcessingImageSDK.ProcessingImage is an object used for work with images inside CIPP, designed to provide basic functionality image processing
    /// </summary>
    [Serializable]
    public class ProcessingImage
    {
        private static List<string> knownExtensionsList = new List<string>() { ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", ".ICO", ".EMF", ".EXIF", ".TIFF", ".TIF", ".WMF" };

        private int sizeX;
        private int sizeY;

        private byte[,] alpha;   // alpha value for masking (if any)
        private byte[,] red;       // red component
        private byte[,] green;       // green component
        private byte[,] blue;       // blue component
        private byte[,] gray;    // gray component (if any)

        public bool grayscale;
        public bool masked;

        [NonSerialized]
        private byte[,] luminance;       // luminance component

        private string path;
        private string name;

        private List<string> watermaks = new List<string>();

        // position in the original image (if this is a subpart)
        private int positionX;
        private int positionY;

        private ImageDependencies imageDependencies;

        /// <summary>
        /// Base image processing class for CIPP
        /// </summary>
        public ProcessingImage()
        {
        }

        public ProcessingImage initialize(String name, int sizeX, int sizeY, bool createOpaqueAlphaChannel = true, int positionX = 0, int positionY = 0)
        {
            this.name = name;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            if (createOpaqueAlphaChannel)
            {
                byte[,] alphaChannel = new byte[sizeY, sizeX];
                for (int i = 0; i < sizeY; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        alphaChannel[i, j] = 255;
                    }
                }
                setAlpha(alphaChannel);
            }

            this.positionX = positionX;
            this.positionY = positionY;
            return this;
        }

        public string getName()
        {
            return name;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public int getSizeX()
        {
            return sizeX;
        }

        public int getSizeY()
        {
            return sizeY;
        }

        public int getPositionX()
        {
            return positionX;
        }

        public int getPositionY()
        {
            return positionY;
        }

        public byte[,] getRed()
        {
            return red;
        }

        public byte[,] getGreen()
        {
            return green;
        }

        public byte[,] getBlue()
        {
            return blue;
        }

        public byte[,] getAlpha()
        {
            return alpha;
        }

        public byte[,] getGray()
        {
            if (gray == null)
            {
                computeGray();
            }
            return gray;
        }

        public byte[,] getLuminance()
        {
            if (luminance == null)
            {
                computeLuminance();
            }
            return luminance;
        }

        public Color getPixel(int x, int y)
        {
            try
            {
                if (grayscale)
                {
                    return Color.FromArgb(alpha[y, x], gray[y, x], gray[y, x], gray[y, x]);
                }
                return Color.FromArgb(alpha[y, x], red[y, x], green[y, x], blue[y, x]);
            }
            catch
            {
                return Color.Black;
            }
        }

        public void setSizeX(int sizeX)
        {
            this.sizeX = sizeX;
        }

        public void setSizeY(int sizeY)
        {
            this.sizeY = sizeY;
        }

        public void setRed(byte[,] red)
        {
            this.red = red;
        }

        public void setGreen(byte[,] green)
        {
            this.green = green;
        }

        public void setBlue(byte[,] blue)
        {
            this.blue = blue;
        }

        public void setAlpha(byte[,] alpha)
        {
            this.alpha = alpha;
        }

        /// <summary>
        /// Sets the gray channel to the image and implicitly sets the image as grayscale and resets the other color channels
        /// </summary>
        /// <param name="gray"></param>
        public void setGray(byte[,] gray)
        {
            this.gray = gray;
            this.grayscale = true;
            this.red = null;
            this.green = null;
            this.blue = null;
            this.luminance = null;
        }

        public void loadImage(Bitmap bitmap)
        {
            positionX = 0;
            positionY = 0;

            sizeX = bitmap.Width;
            sizeY = bitmap.Height;

            grayscale = true;

            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    {
                        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, sizeX, sizeY), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                        alpha = new byte[sizeY, sizeX];
                        red = new byte[sizeY, sizeX];
                        green = new byte[sizeY, sizeX];
                        blue = new byte[sizeY, sizeX];

                        unsafe
                        {
                            Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                            for (int i = 0; i < sizeY; i++)
                            {
                                for (int j = 0; j < sizeX; j++)
                                {
                                    alpha[i, j] = pBase->alpha;
                                    red[i, j] = pBase->red;
                                    green[i, j] = pBase->green;
                                    blue[i, j] = pBase->blue;

                                    if (pBase->alpha != 255)
                                    {
                                        masked = true;
                                    }
                                    if ((pBase->red != pBase->green) || (pBase->green != pBase->blue) || (pBase->red != pBase->blue))
                                    {
                                        grayscale = false;
                                    }

                                    pBase++;
                                }
                            }
                        }
                        bitmap.UnlockBits(bitmapData);
                    } break;
                case PixelFormat.Format24bppRgb:
                    {
                        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, sizeX, sizeY), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                        int remainder = bitmapData.Stride - sizeX * 3; // byte alligned
                        alpha = new byte[sizeY, sizeX];
                        red = new byte[sizeY, sizeX];
                        green = new byte[sizeY, sizeX];
                        blue = new byte[sizeY, sizeX];
                        unsafe
                        {
                            byte* pBase = (byte*)bitmapData.Scan0;
                            for (int i = 0; i < sizeY; i++)
                            {
                                for (int j = 0; j < sizeX; j++)
                                {
                                    alpha[i, j] = 255;
                                    red[i, j] = ((Pixel24Bpp*)pBase)->red;
                                    green[i, j] = ((Pixel24Bpp*)pBase)->green;
                                    blue[i, j] = ((Pixel24Bpp*)pBase)->blue;

                                    if ((((Pixel24Bpp*)pBase)->red != ((Pixel24Bpp*)pBase)->green) ||
                                        (((Pixel24Bpp*)pBase)->green != ((Pixel24Bpp*)pBase)->blue) ||
                                        (((Pixel24Bpp*)pBase)->red != ((Pixel24Bpp*)pBase)->blue))
                                    {
                                        grayscale = false;
                                    }

                                    pBase += 3;
                                }
                                pBase += remainder;
                            }
                        }
                        bitmap.UnlockBits(bitmapData);
                    } break;
                case PixelFormat.Format8bppIndexed:
                    {
                        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, sizeX, sizeY), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                        int remainder = bitmapData.Stride - sizeX * 1; // byte alligned
                        if (bitmap.Palette.Flags == 2) // grayscale: PaletteFlags.GrayScale = 2
                        {
                            alpha = new byte[sizeY, sizeX];
                            red = new byte[sizeY, sizeX];
                            unsafe
                            {
                                Pixel8Bpp* pBase = (Pixel8Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        alpha[i, j] = 255;
                                        red[i, j] = pBase->gray;
                                        pBase++;
                                    }
                                    pBase += remainder;
                                }
                            }
                        }
                        else
                        {
                            alpha = new byte[sizeY, sizeX];
                            red = new byte[sizeY, sizeX];
                            green = new byte[sizeY, sizeX];
                            blue = new byte[sizeY, sizeX];

                            Color[] paleta = bitmap.Palette.Entries;
                            unsafe
                            {
                                Pixel8Bpp* pBase = (Pixel8Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        byte index = pBase->gray;
                                        alpha[i, j] = paleta[index].A;
                                        red[i, j] = paleta[index].R;
                                        green[i, j] = paleta[index].G;
                                        blue[i, j] = paleta[index].B;
                                        pBase++;
                                    }
                                    pBase += remainder;
                                }
                            }
                            grayscale = false;
                        }
                        bitmap.UnlockBits(bitmapData);
                    } break;
                case PixelFormat.Format1bppIndexed:
                    {
                        alpha = new byte[sizeY, sizeX];
                        red = new byte[sizeY, sizeX];
                        for (int i = 0; i < sizeY; i++)
                        {
                            for (int j = 0; j < sizeX; j++)
                            {
                                Color c = bitmap.GetPixel(j, i);
                                alpha[i, j] = c.A;
                                red[i, j] = c.R;
                            }
                        }
                    } break;
                case PixelFormat.Format4bppIndexed:
                    {
                        if (bitmap.Palette.Flags == 2) // grayscale: PaletteFlags.GrayScale = 2
                        {
                            alpha = new byte[sizeY, sizeX];
                            red = new byte[sizeY, sizeX];
                            for (int i = 0; i < sizeY; i++)
                            {
                                for (int j = 0; j < sizeX; j++)
                                {
                                    Color c = bitmap.GetPixel(j, i);
                                    alpha[i, j] = c.A;
                                    red[i, j] = c.R;
                                }
                            }
                        }
                        else
                        {
                            alpha = new byte[sizeY, sizeX];
                            red = new byte[sizeY, sizeX];
                            green = new byte[sizeY, sizeX];
                            blue = new byte[sizeY, sizeX];
                            grayscale = false;

                            for (int i = 0; i < sizeY; i++)
                            {
                                for (int j = 0; j < sizeX; j++)
                                {
                                    Color c = bitmap.GetPixel(j, i);
                                    alpha[i, j] = c.A;
                                    red[i, j] = c.R;
                                    green[i, j] = c.G;
                                    blue[i, j] = c.B;
                                }
                            }
                        }
                    } break;
                default: { } break;
            }

            if (grayscale)
            {
                this.gray = this.red;
                this.red = null;
                this.green = null;
                this.blue = null;
            }
        }

        /// <summary>
        /// Loads Image from specified file name
        /// </summary>
        /// <param name="fileName">Full path of the file to be loaded</param>
        public void loadImage(string fileName)
        {
            Bitmap bitmap = new Bitmap(fileName);
            loadImage(bitmap);
            this.path = fileName;
            this.name = Path.GetFileName(fileName);
        }

        /// <summary>
        /// Saves Image to specified file name using extension filetype. If no extension is provided, image is saved to the default type PNG
        /// </summary>
        /// <param name="fileName">Full path of the file to be loaded</param>
        public void saveImage(string fileName)
        {
            Bitmap bitmap = grayscale ? getBitmap(ProcessingImageBitmapType.AlphaGray) : getBitmap(ProcessingImageBitmapType.AlphaColor);
            String extension = Path.GetExtension(fileName);
            if (extension == null || string.Empty.Equals(extension))
            {
                extension = ".png";
            }
            switch (extension.ToLower())
            {
                case ".png": bitmap.Save(fileName, ImageFormat.Png); break;
                case ".jpg": bitmap.Save(fileName, ImageFormat.Jpeg); break;
                case ".bmp": bitmap.Save(fileName, ImageFormat.Bmp); break;
                case ".gif": bitmap.Save(fileName, ImageFormat.Gif); break;
                case ".ico": bitmap.Save(fileName, ImageFormat.Icon); break;
                case ".emf": bitmap.Save(fileName, ImageFormat.Emf); break;
                case ".exif": bitmap.Save(fileName, ImageFormat.Exif); break;
                case ".tiff": bitmap.Save(fileName, ImageFormat.Tiff); break;
                case ".wmf": bitmap.Save(fileName, ImageFormat.Wmf); break;
                default: bitmap.Save(fileName, ImageFormat.Png); break;
            }
        }

        public void computeGray()
        {
            if (gray == null)
            {
                gray = new byte[sizeY, sizeX];
            }
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    gray[i, j] = (byte)((red[i, j] + green[i, j] + blue[i, j]) / 3);
                }
            }
        }

        public void computeLuminance()
        {
            if (!grayscale)
            {
                if (luminance == null)
                {
                    luminance = new byte[sizeY, sizeX];
                }
                for (int i = 0; i < sizeY; i++)
                {
                    for (int j = 0; j < sizeX; j++)
                    {
                        luminance[i, j] = (byte)(red[i, j] * 0.3f + green[i, j] * 0.59f + blue[i, j] * 0.11f);
                    }
                }
            }
            else
            {
                luminance = (byte[,])(gray.Clone());
            }
        }

        public Bitmap getBitmap(ProcessingImageBitmapType type)
        {
            try
            {
                if (sizeX == 0 || sizeY == 0)
                {
                    return null;
                }
                Bitmap bitmap = new Bitmap(sizeX, sizeY, PixelFormat.Format32bppArgb);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, sizeX, sizeY), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                if (!grayscale)
                {
                    switch (type)
                    {
                        case ProcessingImageBitmapType.Alpha:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = 255;
                                        pBase->red = alpha[i, j];
                                        pBase->green = alpha[i, j];
                                        pBase->blue = alpha[i, j];
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.AlphaColor:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = alpha[i, j];
                                        pBase->red = red[i, j];
                                        pBase->green = green[i, j];
                                        pBase->blue = blue[i, j];
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.AlphaRed:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = alpha[i, j];
                                        pBase->red = red[i, j];
                                        pBase->green = 0;
                                        pBase->blue = 0;
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.AlphaGreen:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = alpha[i, j];
                                        pBase->red = 0;
                                        pBase->green = green[i, j];
                                        pBase->blue = 0;
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.AlphaBlue:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = alpha[i, j];
                                        pBase->red = 0;
                                        pBase->green = 0;
                                        pBase->blue = blue[i, j];
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.AlphaRedGreen:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = alpha[i, j];
                                        pBase->red = red[i, j];
                                        pBase->green = green[i, j];
                                        pBase->blue = 0;
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.AlphaRedBlue:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = alpha[i, j];
                                        pBase->red = red[i, j];
                                        pBase->green = 0;
                                        pBase->blue = blue[i, j];
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.AlphaGreenBlue:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = alpha[i, j];
                                        pBase->red = 0;
                                        pBase->green = green[i, j];
                                        pBase->blue = blue[i, j];
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.Red:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = 255;
                                        pBase->red = red[i, j];
                                        pBase->green = 0;
                                        pBase->blue = 0;
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.Green:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = 255;
                                        pBase->red = 0;
                                        pBase->green = green[i, j];
                                        pBase->blue = 0;
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.Blue:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = 255;
                                        pBase->red = 0;
                                        pBase->green = 0;
                                        pBase->blue = blue[i, j];
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.RedGreen:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = 255;
                                        pBase->red = red[i, j];
                                        pBase->green = green[i, j];
                                        pBase->blue = 0;
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.RedBlue:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = 255;
                                        pBase->red = red[i, j];
                                        pBase->green = 0;
                                        pBase->blue = blue[i, j];
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.GreenBlue:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = 255;
                                        pBase->red = 0;
                                        pBase->green = green[i, j];
                                        pBase->blue = blue[i, j];
                                        pBase++;
                                    }
                                }
                            } break;

                        case ProcessingImageBitmapType.AlphaGray:
                            {
                                if (gray == null)
                                {
                                    this.computeGray();
                                }
                                unsafe
                                {
                                    Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                    for (int i = 0; i < sizeY; i++)
                                    {
                                        for (int j = 0; j < sizeX; j++)
                                        {
                                            pBase->alpha = alpha[i, j];
                                            pBase->red = gray[i, j];
                                            pBase->green = gray[i, j];
                                            pBase->blue = gray[i, j];
                                            pBase++;
                                        }
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.Gray:
                            {
                                if (gray == null)
                                {
                                    this.computeGray();
                                }
                                unsafe
                                {
                                    Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                    for (int i = 0; i < sizeY; i++)
                                    {
                                        for (int j = 0; j < sizeX; j++)
                                        {
                                            pBase->alpha = 255;
                                            pBase->red = gray[i, j];
                                            pBase->green = gray[i, j];
                                            pBase->blue = gray[i, j];
                                            pBase++;
                                        }
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.AlphaLuminance:
                            {
                                if (luminance == null)
                                {
                                    this.computeLuminance();
                                }
                                unsafe
                                {
                                    Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                    for (int i = 0; i < sizeY; i++)
                                    {
                                        for (int j = 0; j < sizeX; j++)
                                        {
                                            pBase->alpha = alpha[i, j];
                                            pBase->red = luminance[i, j];
                                            pBase->green = luminance[i, j];
                                            pBase->blue = luminance[i, j];
                                            pBase++;
                                        }
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.Luminance:
                            {
                                if (luminance == null)
                                {
                                    this.computeLuminance();
                                }
                                unsafe
                                {
                                    Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                    for (int i = 0; i < sizeY; i++)
                                    {
                                        for (int j = 0; j < sizeX; j++)
                                        {
                                            pBase->alpha = 255;
                                            pBase->red = luminance[i, j];
                                            pBase->green = luminance[i, j];
                                            pBase->blue = luminance[i, j];
                                            pBase++;
                                        }
                                    }
                                }
                            } break;
                        default:
                            {
                                if (grayscale)
                                {
                                    unsafe
                                    {
                                        Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                        for (int i = 0; i < sizeY; i++)
                                        {
                                            for (int j = 0; j < sizeX; j++)
                                            {
                                                pBase->alpha = 255;
                                                pBase->red = gray[i, j];
                                                pBase->green = gray[i, j];
                                                pBase->blue = gray[i, j];
                                                pBase++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    unsafe
                                    {
                                        Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                        for (int i = 0; i < sizeY; i++)
                                        {
                                            for (int j = 0; j < sizeX; j++)
                                            {
                                                pBase->alpha = 255;
                                                pBase->red = red[i, j];
                                                pBase->green = green[i, j];
                                                pBase->blue = blue[i, j];
                                                pBase++;
                                            }
                                        }
                                    }
                                }
                            } break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case ProcessingImageBitmapType.Alpha:
                            unsafe
                            {
                                Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        pBase->alpha = 255;
                                        pBase->red = alpha[i, j];
                                        pBase->green = alpha[i, j];
                                        pBase->blue = alpha[i, j];
                                        pBase++;
                                    }
                                }
                            } break;
                        case ProcessingImageBitmapType.AlphaGray:
                            {
                                if (gray == null)
                                {
                                    this.computeGray();
                                }
                                unsafe
                                {
                                    Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                    for (int i = 0; i < sizeY; i++)
                                    {
                                        for (int j = 0; j < sizeX; j++)
                                        {
                                            pBase->alpha = alpha[i, j];
                                            pBase->red = gray[i, j];
                                            pBase->green = gray[i, j];
                                            pBase->blue = gray[i, j];
                                            pBase++;
                                        }
                                    }
                                }
                            } break;
                        default:
                            {
                                if (gray == null)
                                {
                                    this.computeGray();
                                }
                                unsafe
                                {
                                    Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                    for (int i = 0; i < sizeY; i++)
                                    {
                                        for (int j = 0; j < sizeX; j++)
                                        {
                                            pBase->alpha = 255;
                                            pBase->red = gray[i, j];
                                            pBase->green = gray[i, j];
                                            pBase->blue = gray[i, j];
                                            pBase++;
                                        }
                                    }
                                }
                            } break;
                    }
                }

                bitmap.UnlockBits(bitmapData);
                return bitmap;
            }
            catch
            {
                throw new Exception("Could not convert to bitmap.");
            }
        }

        public Bitmap getPreviewBitmap(int sizeX, int sizeY)
        {
            try
            {
                // take the best ratio
                float delta = (float)this.sizeX / sizeX > (float)this.sizeY / sizeY ? (float)this.sizeX / sizeX : (float)this.sizeY / sizeY;
                float currentX, currentY = 0;

                int newSizeX = (int)(this.sizeX / delta);
                int newSizeY = (int)(this.sizeY / delta);
                Bitmap bitmap = new Bitmap(newSizeX, newSizeY, PixelFormat.Format32bppArgb);

                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, newSizeX, newSizeY), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                unsafe
                {
                    Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                    if (!grayscale)
                    {
                        for (int i = 0; i < newSizeY; i++)
                        {
                            currentX = 0;
                            for (int j = 0; j < newSizeX; j++)
                            {
                                pBase->alpha = alpha[(int)currentY, (int)currentX];
                                pBase->red = red[(int)currentY, (int)currentX];
                                pBase->green = green[(int)currentY, (int)currentX];
                                pBase->blue = blue[(int)currentY, (int)currentX];

                                currentX += delta;
                                pBase++;
                            }
                            currentY += delta;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < newSizeY; i++)
                        {
                            currentX = 0;
                            for (int j = 0; j < newSizeX; j++)
                            {
                                pBase->alpha = alpha[(int)currentY, (int)currentX];
                                pBase->red = gray[(int)currentY, (int)currentX];
                                pBase->green = gray[(int)currentY, (int)currentX];
                                pBase->blue = gray[(int)currentY, (int)currentX];

                                currentX += delta;
                                pBase++;
                            }
                            currentY += delta;
                        }
                    }
                }

                bitmap.UnlockBits(bitmapData);
                return bitmap;
            }
            catch
            {
                throw new Exception("Could not convert to preview bitmap.");
            }
        }

        public List<string> getWatermarks()
        {
            return watermaks;
        }

        public void addWatermark(string watermark)
        {
            watermaks.Add(watermark);
        }

        public void setWatermarks(List<string> watermarks)
        {
            this.watermaks = watermarks;
        }

        public static void copyAttributes(ProcessingImage source, ProcessingImage target, bool cloneAlpha = true)
        {
            target.grayscale = source.grayscale;
            target.masked = source.masked;
            target.sizeX = source.sizeX;
            target.sizeY = source.sizeY;
            target.positionX = source.positionX;
            target.positionY = source.positionY;
            target.imageDependencies = source.imageDependencies;

            target.path = null;
            target.name = source.name;

            target.watermaks.Clear();
            target.watermaks.AddRange(source.watermaks);
            if (cloneAlpha)
            {
                target.alpha = (byte[,])source.getAlpha().Clone();
            }
        }

        public void copyAttributes(ProcessingImage originalImage)
        {
            copyAttributes(originalImage, this, false);
        }

        public void copyAttributesAndAlpha(ProcessingImage originalImage)
        {
            copyAttributes(originalImage, this, true);
        }

        public ProcessingImage clone(bool cloneAlpha = true)
        {
            ProcessingImage processingImage = new ProcessingImage();
            copyAttributes(this, processingImage, cloneAlpha);
            if (!grayscale)
            {
                processingImage.red = (byte[,])this.red.Clone();
                processingImage.green = (byte[,])this.green.Clone();
                processingImage.blue = (byte[,])this.blue.Clone();
            }
            else
            {
                processingImage.grayscale = true;
                processingImage.gray = (byte[,])this.gray.Clone();
            }
            return processingImage;
        }

        public ProcessingImage cloneAndSubstituteAlpha(byte[,] alphaChannel)
        {
            ProcessingImage processingImage = this.clone(false);
            processingImage.alpha = alphaChannel;
            return processingImage;
        }

        public ProcessingImage blankClone()
        {
            ProcessingImage processingImage = new ProcessingImage();
            copyAttributes(this, processingImage, false);

            processingImage.alpha = new byte[sizeY, sizeX];
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    processingImage.alpha[i, j] = 255;
                }
            }
            if (!grayscale)
            {
                processingImage.red = new byte[sizeY, sizeX];
                processingImage.green = new byte[sizeY, sizeX];
                processingImage.blue = new byte[sizeY, sizeX];
            }
            else
            {
                processingImage.grayscale = true;
                processingImage.gray = new byte[sizeY, sizeX];
            }
            return processingImage;
        }

        public ProcessingImage convolution(int[,] matrix)
        {
            ProcessingImage processingImage = new ProcessingImage();
            processingImage.copyAttributesAndAlpha(this);

            if (grayscale)
            {
                int[,] convolvedGray = ProcessingImageUtils.delayedConvolution(gray, matrix);
                processingImage.gray = ProcessingImageUtils.truncateToDisplay(convolvedGray);
            }
            else
            {
                int[,] convolvedRed = ProcessingImageUtils.delayedConvolution(red, matrix);
                int[,] convolvedGreen = ProcessingImageUtils.delayedConvolution(green, matrix);
                int[,] convolvedBlue = ProcessingImageUtils.delayedConvolution(blue, matrix);
                processingImage.red = ProcessingImageUtils.truncateToDisplay(convolvedRed);
                processingImage.green = ProcessingImageUtils.truncateToDisplay(convolvedGreen);
                processingImage.blue = ProcessingImageUtils.truncateToDisplay(convolvedBlue);
            }

            return processingImage;
        }

        public ProcessingImage convolution(float[,] matrix)
        {
            ProcessingImage processingImage = new ProcessingImage();
            processingImage.copyAttributesAndAlpha(this);

            if (grayscale)
            {
                float[,] convolvedGray = ProcessingImageUtils.delayedConvolution(gray, matrix);
                processingImage.gray = ProcessingImageUtils.truncateToDisplay(convolvedGray);
            }
            else
            {
                float[,] convolvedRed = ProcessingImageUtils.delayedConvolution(red, matrix);
                float[,] convolvedGreen = ProcessingImageUtils.delayedConvolution(green, matrix);
                float[,] convolvedBlue = ProcessingImageUtils.delayedConvolution(blue, matrix);
                processingImage.red = ProcessingImageUtils.truncateToDisplay(convolvedRed);
                processingImage.green = ProcessingImageUtils.truncateToDisplay(convolvedGreen);
                processingImage.blue = ProcessingImageUtils.truncateToDisplay(convolvedBlue);
            }

            return processingImage;
        }

        public ProcessingImage mirroredMarginConvolution(float[,] matrix)
        {
            ProcessingImage processingImage = new ProcessingImage();
            processingImage.copyAttributesAndAlpha(this);
            if (grayscale)
            {
                float[,] convolvedGray = ProcessingImageUtils.mirroredMarginConvolution(gray, matrix);
                processingImage.gray = ProcessingImageUtils.truncateToDisplay(convolvedGray);
            }
            else
            {
                float[,] convolvedRed = ProcessingImageUtils.mirroredMarginConvolution(red, matrix);
                float[,] convolvedGreen = ProcessingImageUtils.mirroredMarginConvolution(green, matrix);
                float[,] convolvedBlue = ProcessingImageUtils.mirroredMarginConvolution(blue, matrix);
                processingImage.red = ProcessingImageUtils.truncateToDisplay(convolvedRed);
                processingImage.green = ProcessingImageUtils.truncateToDisplay(convolvedGreen);
                processingImage.blue = ProcessingImageUtils.truncateToDisplay(convolvedBlue);
            }
            return processingImage;
        }

        public ProcessingImage mirroredMarginConvolution(int[,] matrix)
        {
            ProcessingImage processingImage = new ProcessingImage();
            processingImage.copyAttributesAndAlpha(this);
            if (grayscale)
            {
                int[,] convolvedGray = ProcessingImageUtils.mirroredMarginConvolution(gray, matrix);
                processingImage.gray = ProcessingImageUtils.truncateToDisplay(convolvedGray);
            }
            else
            {
                int[,] convolvedRed = ProcessingImageUtils.mirroredMarginConvolution(red, matrix);
                int[,] convolvedGreen = ProcessingImageUtils.mirroredMarginConvolution(green, matrix);
                int[,] convolvedBlue = ProcessingImageUtils.mirroredMarginConvolution(blue, matrix);
                processingImage.red = ProcessingImageUtils.truncateToDisplay(convolvedRed);
                processingImage.green = ProcessingImageUtils.truncateToDisplay(convolvedGreen);
                processingImage.blue = ProcessingImageUtils.truncateToDisplay(convolvedBlue);
            }
            return processingImage;
        }

        public static List<string> getKnownExtensions()
        {
            return knownExtensionsList;
        }

        /// <summary>
        /// Creates an array of images
        /// </summary>
        /// <param name="imageDependencies">defines image dependencies</param>
        /// <param name="subParts">number of subparts to divide</param>
        /// <returns>An array of images</returns>
        public ProcessingImage[] split(ImageDependencies imageDependencies, int subParts)
        {
            if (imageDependencies.left == -1)
            {
                return null;
            }
            if ((imageDependencies.left + imageDependencies.right) * subParts >= sizeX)
            {
                return null;
            }
            if ((imageDependencies.top + imageDependencies.bottom) * subParts >= sizeY)
            {
                return null;
            }

            ProcessingImage[] processingImage = new ProcessingImage[subParts];
            for (int i = 0; i < subParts; i++)
            {
                processingImage[i] = new ProcessingImage();
            }

            int stepSize = sizeX / subParts;
            int size = stepSize + imageDependencies.left + imageDependencies.right;
            int start = 0;

            for (int i = 0; i < subParts; i++)
            {
                processingImage[i].imageDependencies = imageDependencies;
                processingImage[i].positionX = start;
                processingImage[i].positionY = 0;
                if (i == 0)
                {
                    processingImage[i].sizeX = stepSize + imageDependencies.right;
                }
                else
                {
                    if (i == subParts - 1)
                    {
                        processingImage[i].sizeX = sizeX - (subParts - 1) * stepSize + imageDependencies.left;
                    }
                    else
                    {
                        processingImage[i].sizeX = size;
                    }
                }
                processingImage[i].sizeY = sizeY;


                byte[,] al = new byte[processingImage[i].sizeY, processingImage[i].sizeX];
                if (i == 0)
                {
                    for (int y = 0; y < processingImage[i].sizeY; y++)
                    {
                        for (int x = 0; x < processingImage[i].sizeX; x++)
                        {
                            al[y, x] = alpha[y, x];
                        }
                    }
                }
                else
                {
                    for (int y = 0; y < processingImage[i].sizeY; y++)
                    {
                        for (int x = 0; x < processingImage[i].sizeX; x++)
                        {
                            al[y, x] = alpha[y, x - imageDependencies.left + start];
                        }
                    }
                }

                processingImage[i].alpha = al;

                if (grayscale)
                {
                    processingImage[i].grayscale = true;
                    byte[,] gr = new byte[processingImage[i].sizeY, processingImage[i].sizeX];
                    if (i == 0)
                    {
                        for (int y = 0; y < processingImage[i].sizeY; y++)
                        {
                            for (int x = 0; x < processingImage[i].sizeX; x++)
                            {
                                gr[y, x] = gray[y, x];
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < processingImage[i].sizeY; y++)
                        {
                            for (int x = 0; x < processingImage[i].sizeX; x++)
                            {
                                gr[y, x] = gray[y, x - imageDependencies.left + start];
                            }
                        }
                    }
                    processingImage[i].gray = gr;
                }
                else
                {
                    byte[,] red = new byte[processingImage[i].sizeY, processingImage[i].sizeX];
                    byte[,] green = new byte[processingImage[i].sizeY, processingImage[i].sizeX];
                    byte[,] blue = new byte[processingImage[i].sizeY, processingImage[i].sizeX];

                    if (i == 0)
                    {
                        for (int y = 0; y < processingImage[i].sizeY; y++)
                        {
                            for (int x = 0; x < processingImage[i].sizeX; x++)
                            {
                                red[y, x] = red[y, x];
                                green[y, x] = green[y, x];
                                blue[y, x] = blue[y, x];
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < processingImage[i].sizeY; y++)
                        {
                            for (int x = 0; x < processingImage[i].sizeX; x++)
                            {
                                red[y, x] = red[y, x - imageDependencies.left + start];
                                green[y, x] = green[y, x - imageDependencies.left + start];
                                blue[y, x] = blue[y, x - imageDependencies.left + start];
                            }
                        }
                    }
                    processingImage[i].red = red;
                    processingImage[i].green = green;
                    processingImage[i].blue = blue;
                }

                start += stepSize;
            }
            return processingImage;
        }

        public void join(ProcessingImage subPart)
        {
            try
            {
                if (subPart.positionX == 0)
                {
                    for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                    {
                        for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                        {
                            alpha[i + subPart.positionY, j + subPart.positionX] = subPart.alpha[i, j];
                        }
                    }

                    this.watermaks = subPart.watermaks;
                }
                else
                {
                    for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                    {
                        for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                        {
                            alpha[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.alpha[i, j];
                        }
                    }
                }
                if (subPart.grayscale)
                {
                    if (!grayscale)
                    {
                        red = null;
                        green = null;
                        blue = null;
                        gray = new byte[this.sizeY, this.sizeX];
                        grayscale = true;
                    }
                    if (subPart.positionX == 0)
                    {
                        for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                        {
                            for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                            {
                                gray[i + subPart.positionY, j + subPart.positionX] = subPart.gray[i, j];
                            }
                        }
                    }
                    else
                    {
                        for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                        {
                            for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                            {
                                gray[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.gray[i, j];
                            }
                        }
                    }
                }
                else
                {
                    if (grayscale)
                    {
                        gray = null;
                        red = new byte[this.sizeY, this.sizeX];
                        green = new byte[this.sizeY, this.sizeX];
                        blue = new byte[this.sizeY, this.sizeX];
                        grayscale = false;
                    }
                    if (subPart.positionX == 0)
                    {
                        for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                        {
                            for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                            {
                                red[i + subPart.positionY, j + subPart.positionX] = subPart.red[i, j];
                                green[i + subPart.positionY, j + subPart.positionX] = subPart.green[i, j];
                                blue[i + subPart.positionY, j + subPart.positionX] = subPart.blue[i, j];
                            }
                        }
                    }
                    else
                    {
                        for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                        {
                            for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                            {
                                red[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.red[i, j];
                                green[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.green[i, j];
                                blue[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.blue[i, j];
                            }
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Image join failed");
            }
        }

        public override string ToString()
        {
            return name;
        }

    }
}