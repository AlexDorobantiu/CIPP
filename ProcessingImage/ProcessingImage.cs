using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.IO;

namespace ProcessingImageSDK
{
    /// <summary>
    ///  ProcessingImageSDK.ProcessingImage is an object used for work with images inside CIPP, designed to provide basic functionality image processing
    /// </summary>
    [Serializable]
    public class ProcessingImage
    {
        private static List<string> knownExtensionsList = new List<string>() { ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", ".ICO", ".EMF", ".EXIF", ".TIFF", ".TIF", ".WMF" };

        public bool grayscale;
        public bool masked;

        private int sizeX;
        private int sizeY;

        //position in the original image (if this is a subpart)
        private int positionX;
        private int positionY;
        private ImageDependencies imageDependencies;

        private byte[,] alpha;   //alpha value for masking (if any)
        private byte[,] r;       //red component
        private byte[,] g;       //green component
        private byte[,] b;       //blue component
        private byte[,] gray;    //gray component (if any)

        [NonSerialized]
        private byte[,] luminance;       //luminance component

        private string path;
        private string name;

        private List<string> watermaks = new List<string>();

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

        public void setName(string newName)
        {
            name = newName;
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
            return r;
        }

        public byte[,] getGreen()
        {
            return g;
        }

        public byte[,] getBlue()
        {
            return b;
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
                return Color.FromArgb(alpha[y, x], r[y, x], g[y, x], b[y, x]);
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
            this.r = red;
        }

        public void setGreen(byte[,] green)
        {
            this.g = green;
        }

        public void setBlue(byte[,] blue)
        {
            this.b = blue;
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
            this.r = null;
            this.g = null;
            this.b = null;
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
                        r = new byte[sizeY, sizeX];
                        g = new byte[sizeY, sizeX];
                        b = new byte[sizeY, sizeX];

                        unsafe
                        {
                            Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                            for (int i = 0; i < sizeY; i++)
                            {
                                for (int j = 0; j < sizeX; j++)
                                {
                                    alpha[i, j] = pBase->alpha;
                                    r[i, j] = pBase->red;
                                    g[i, j] = pBase->green;
                                    b[i, j] = pBase->blue;

                                    if (pBase->alpha != 255) masked = true;
                                    if ((pBase->red != pBase->green) || (pBase->green != pBase->blue) || (pBase->red != pBase->blue)) grayscale = false;

                                    pBase++;
                                }
                            }
                        }
                        bitmap.UnlockBits(bitmapData);
                    } break;
                case PixelFormat.Format24bppRgb:
                    {
                        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, sizeX, sizeY), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                        int remainder = bitmapData.Stride - sizeX * 3; //aliniament la byte
                        alpha = new byte[sizeY, sizeX];
                        r = new byte[sizeY, sizeX];
                        g = new byte[sizeY, sizeX];
                        b = new byte[sizeY, sizeX];
                        unsafe
                        {
                            byte* pBase = (byte*)bitmapData.Scan0;
                            for (int i = 0; i < sizeY; i++)
                            {
                                for (int j = 0; j < sizeX; j++)
                                {
                                    alpha[i, j] = 255;
                                    r[i, j] = ((Pixel24Bpp*)pBase)->red;
                                    g[i, j] = ((Pixel24Bpp*)pBase)->green;
                                    b[i, j] = ((Pixel24Bpp*)pBase)->blue;

                                    if ((((Pixel24Bpp*)pBase)->red != ((Pixel24Bpp*)pBase)->green) ||
                                        (((Pixel24Bpp*)pBase)->green != ((Pixel24Bpp*)pBase)->blue) ||
                                        (((Pixel24Bpp*)pBase)->red != ((Pixel24Bpp*)pBase)->blue))
                                        grayscale = false;

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
                            r = new byte[sizeY, sizeX];
                            unsafe
                            {
                                Pixel8Bpp* pBase = (Pixel8Bpp*)bitmapData.Scan0;
                                for (int i = 0; i < sizeY; i++)
                                {
                                    for (int j = 0; j < sizeX; j++)
                                    {
                                        alpha[i, j] = 255;
                                        r[i, j] = pBase->gray;
                                        pBase++;
                                    }
                                    pBase += remainder;
                                }
                            }
                        }
                        else
                        {
                            alpha = new byte[sizeY, sizeX];
                            r = new byte[sizeY, sizeX];
                            g = new byte[sizeY, sizeX];
                            b = new byte[sizeY, sizeX];

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
                                        r[i, j] = paleta[index].R;
                                        g[i, j] = paleta[index].G;
                                        b[i, j] = paleta[index].B;
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
                        r = new byte[sizeY, sizeX];
                        for (int i = 0; i < sizeY; i++)
                        {
                            for (int j = 0; j < sizeX; j++)
                            {
                                Color c = bitmap.GetPixel(j, i);
                                alpha[i, j] = c.A;
                                r[i, j] = c.R;
                            }
                        }
                    } break;
                case PixelFormat.Format4bppIndexed:
                    {
                        if (bitmap.Palette.Flags == 2) // grayscale: PaletteFlags.GrayScale = 2
                        {
                            alpha = new byte[sizeY, sizeX];
                            r = new byte[sizeY, sizeX];
                            for (int i = 0; i < sizeY; i++)
                            {
                                for (int j = 0; j < sizeX; j++)
                                {
                                    Color c = bitmap.GetPixel(j, i);
                                    alpha[i, j] = c.A;
                                    r[i, j] = c.R;
                                }
                            }
                        }
                        else
                        {
                            alpha = new byte[sizeY, sizeX];
                            r = new byte[sizeY, sizeX];
                            g = new byte[sizeY, sizeX];
                            b = new byte[sizeY, sizeX];
                            grayscale = false;

                            for (int i = 0; i < sizeY; i++)
                            {
                                for (int j = 0; j < sizeX; j++)
                                {
                                    Color c = bitmap.GetPixel(j, i);
                                    alpha[i, j] = c.A;
                                    r[i, j] = c.R;
                                    g[i, j] = c.G;
                                    b[i, j] = c.B;
                                }
                            }
                        }
                    } break;
                default: { } break;
            }

            if (grayscale)
            {
                this.gray = this.r;
                this.r = null;
                this.g = null;
                this.b = null;
            }
        }

        /// <summary>
        /// Loads Image from specified file name
        /// </summary>
        /// <param name="fileName">Full path of the file to be loaded</param>
        public void loadImage(string fileName)
        {
            Bitmap b = new Bitmap(fileName);
            loadImage(b);
            this.path = fileName;
            this.name = Path.GetFileName(fileName);
        }

        /// <summary>
        /// Saves Image to specified file name using extension filetype. If no extension is provided, image is saved to the default type PNG
        /// </summary>
        /// <param name="fileName">Full path of the file to be loaded</param>
        public void saveImage(string fileName)
        {
            Bitmap b = grayscale ? getBitmap(ProcessingImageBitmapType.AlphaGray) : getBitmap(ProcessingImageBitmapType.AlphaColor);
            String extension = Path.GetExtension(fileName);
            if (extension == null || string.Empty.Equals(extension))
            {
                extension = ".png";
            }
            switch (extension.ToLower())
            {
                case ".png": b.Save(fileName, ImageFormat.Png); break;
                case ".jpg": b.Save(fileName, ImageFormat.Jpeg); break;
                case ".bmp": b.Save(fileName, ImageFormat.Bmp); break;
                case ".gif": b.Save(fileName, ImageFormat.Gif); break;
                case ".ico": b.Save(fileName, ImageFormat.Icon); break;
                case ".emf": b.Save(fileName, ImageFormat.Emf); break;
                case ".exif": b.Save(fileName, ImageFormat.Exif); break;
                case ".tiff": b.Save(fileName, ImageFormat.Tiff); break;
                case ".wmf": b.Save(fileName, ImageFormat.Wmf); break;
                default: b.Save(fileName, ImageFormat.Png); break;
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
                    gray[i, j] = (byte)((r[i, j] + g[i, j] + b[i, j]) / 3);
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
                        luminance[i, j] = (byte)(r[i, j] * 0.3f + g[i, j] * 0.59f + b[i, j] * 0.11f);
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
                                        pBase->red = r[i, j];
                                        pBase->green = g[i, j];
                                        pBase->blue = b[i, j];
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
                                        pBase->red = r[i, j];
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
                                        pBase->green = g[i, j];
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
                                        pBase->blue = b[i, j];
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
                                        pBase->red = r[i, j];
                                        pBase->green = g[i, j];
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
                                        pBase->red = r[i, j];
                                        pBase->green = 0;
                                        pBase->blue = b[i, j];
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
                                        pBase->green = g[i, j];
                                        pBase->blue = b[i, j];
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
                                        pBase->red = r[i, j];
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
                                        pBase->green = g[i, j];
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
                                        pBase->blue = b[i, j];
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
                                        pBase->red = r[i, j];
                                        pBase->green = g[i, j];
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
                                        pBase->red = r[i, j];
                                        pBase->green = 0;
                                        pBase->blue = b[i, j];
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
                                        pBase->green = g[i, j];
                                        pBase->blue = b[i, j];
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
                                            pBase->red = r[i, j];
                                            pBase->green = g[i, j];
                                            pBase->blue = b[i, j];
                                            pBase++;
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
                                if (gray == null) this.computeGray();
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
                                if (gray == null) this.computeGray();
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
                                pBase->red = r[(int)currentY, (int)currentX];
                                pBase->green = g[(int)currentY, (int)currentX];
                                pBase->blue = b[(int)currentY, (int)currentX];

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
            ProcessingImage pi = new ProcessingImage();
            copyAttributes(this, pi, cloneAlpha);
            if (!grayscale)
            {
                pi.r = (byte[,])this.r.Clone();
                pi.g = (byte[,])this.g.Clone();
                pi.b = (byte[,])this.b.Clone();
            }
            else
            {
                pi.grayscale = true;
                pi.gray = (byte[,])this.gray.Clone();
            }
            return pi;
        }

        public ProcessingImage cloneAndSubstituteAlpha(byte[,] alphaChannel)
        {
            ProcessingImage pi = this.clone(false);
            pi.alpha = alphaChannel;
            return pi;
        }

        public ProcessingImage blankClone()
        {
            ProcessingImage pi = new ProcessingImage();
            copyAttributes(this, pi, false);

            pi.alpha = new byte[sizeY, sizeX];
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    pi.alpha[i, j] = 255;
                }
            }
            if (!grayscale)
            {
                pi.r = new byte[sizeY, sizeX];
                pi.g = new byte[sizeY, sizeX];
                pi.b = new byte[sizeY, sizeX];
            }
            else
            {
                pi.grayscale = true;
                pi.gray = new byte[sizeY, sizeX];
            }
            return pi;
        }

        public ProcessingImage convolution(int[,] matrix)
        {
            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributesAndAlpha(this);

            if (grayscale)
            {
                int[,] convolvedGray = ProcessingImageUtils.delayedConvolution(gray, matrix);
                pi.gray = ProcessingImageUtils.truncateToDisplay(convolvedGray);
            }
            else
            {
                int[,] convolvedRed = ProcessingImageUtils.delayedConvolution(r, matrix);
                int[,] convolvedGreen = ProcessingImageUtils.delayedConvolution(g, matrix);
                int[,] convolvedBlue = ProcessingImageUtils.delayedConvolution(b, matrix);
                pi.r = ProcessingImageUtils.truncateToDisplay(convolvedRed);
                pi.g = ProcessingImageUtils.truncateToDisplay(convolvedGreen);
                pi.b = ProcessingImageUtils.truncateToDisplay(convolvedBlue);
            }

            return pi;
        }

        public ProcessingImage convolution(float[,] matrix)
        {
            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributesAndAlpha(this);

            if (grayscale)
            {
                float[,] convolvedGray = ProcessingImageUtils.delayedConvolution(gray, matrix);
                pi.gray = ProcessingImageUtils.truncateToDisplay(convolvedGray);
            }
            else
            {
                float[,] convolvedRed = ProcessingImageUtils.delayedConvolution(r, matrix);
                float[,] convolvedGreen = ProcessingImageUtils.delayedConvolution(g, matrix);
                float[,] convolvedBlue = ProcessingImageUtils.delayedConvolution(b, matrix);
                pi.r = ProcessingImageUtils.truncateToDisplay(convolvedRed);
                pi.g = ProcessingImageUtils.truncateToDisplay(convolvedGreen);
                pi.b = ProcessingImageUtils.truncateToDisplay(convolvedBlue);
            }

            return pi;
        }

        public ProcessingImage mirroredMarginConvolution(float[,] matrix)
        {
            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributesAndAlpha(this);
            if (grayscale)
            {
                float[,] convolvedGray = ProcessingImageUtils.mirroredMarginConvolution(gray, matrix);
                pi.gray = ProcessingImageUtils.truncateToDisplay(convolvedGray);
            }
            else
            {
                float[,] convolvedRed = ProcessingImageUtils.mirroredMarginConvolution(r, matrix);
                float[,] convolvedGreen = ProcessingImageUtils.mirroredMarginConvolution(g, matrix);
                float[,] convolvedBlue = ProcessingImageUtils.mirroredMarginConvolution(b, matrix);
                pi.r = ProcessingImageUtils.truncateToDisplay(convolvedRed);
                pi.g = ProcessingImageUtils.truncateToDisplay(convolvedGreen);
                pi.b = ProcessingImageUtils.truncateToDisplay(convolvedBlue);
            }
            return pi;
        }

        public ProcessingImage mirroredMarginConvolution(int[,] matrix)
        {
            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributesAndAlpha(this);
            if (grayscale)
            {
                int[,] convolvedGray = ProcessingImageUtils.mirroredMarginConvolution(gray, matrix);
                pi.gray = ProcessingImageUtils.truncateToDisplay(convolvedGray);
            }
            else
            {
                int[,] convolvedRed = ProcessingImageUtils.mirroredMarginConvolution(r, matrix);
                int[,] convolvedGreen = ProcessingImageUtils.mirroredMarginConvolution(g, matrix);
                int[,] convolvedBlue = ProcessingImageUtils.mirroredMarginConvolution(b, matrix);
                pi.r = ProcessingImageUtils.truncateToDisplay(convolvedRed);
                pi.g = ProcessingImageUtils.truncateToDisplay(convolvedGreen);
                pi.b = ProcessingImageUtils.truncateToDisplay(convolvedBlue);
            }
            return pi;
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

            ProcessingImage[] pi = new ProcessingImage[subParts];
            for (int i = 0; i < subParts; i++)
            {
                pi[i] = new ProcessingImage();
            }

            int stepSize = sizeX / subParts;
            int size = stepSize + imageDependencies.left + imageDependencies.right;
            int start = 0;

            for (int i = 0; i < subParts; i++)
            {
                pi[i].imageDependencies = imageDependencies;
                pi[i].positionX = start;
                pi[i].positionY = 0;
                if (i == 0)
                {
                    pi[i].sizeX = stepSize + imageDependencies.right;
                }
                else
                    if (i == subParts - 1)
                    {
                        pi[i].sizeX = sizeX - (subParts - 1) * stepSize + imageDependencies.left;
                    }
                    else
                    {
                        pi[i].sizeX = size;
                    }

                pi[i].sizeY = sizeY;


                byte[,] al = new byte[pi[i].sizeY, pi[i].sizeX];
                if (i == 0)
                {
                    for (int y = 0; y < pi[i].sizeY; y++)
                    {
                        for (int x = 0; x < pi[i].sizeX; x++)
                        {
                            al[y, x] = alpha[y, x];
                        }
                    }
                }
                else
                {
                    for (int y = 0; y < pi[i].sizeY; y++)
                    {
                        for (int x = 0; x < pi[i].sizeX; x++)
                        {
                            al[y, x] = alpha[y, x - imageDependencies.left + start];
                        }
                    }
                }

                pi[i].alpha = al;

                if (grayscale)
                {
                    pi[i].grayscale = true;
                    byte[,] gr = new byte[pi[i].sizeY, pi[i].sizeX];
                    if (i == 0)
                    {
                        for (int y = 0; y < pi[i].sizeY; y++)
                        {
                            for (int x = 0; x < pi[i].sizeX; x++)
                            {
                                gr[y, x] = gray[y, x];
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < pi[i].sizeY; y++)
                        {
                            for (int x = 0; x < pi[i].sizeX; x++)
                            {
                                gr[y, x] = gray[y, x - imageDependencies.left + start];
                            }
                        }
                    }
                    pi[i].gray = gr;
                }
                else
                {
                    byte[,] red = new byte[pi[i].sizeY, pi[i].sizeX];
                    byte[,] green = new byte[pi[i].sizeY, pi[i].sizeX];
                    byte[,] blue = new byte[pi[i].sizeY, pi[i].sizeX];

                    if (i == 0)
                    {
                        for (int y = 0; y < pi[i].sizeY; y++)
                        {
                            for (int x = 0; x < pi[i].sizeX; x++)
                            {
                                red[y, x] = r[y, x];
                                green[y, x] = g[y, x];
                                blue[y, x] = b[y, x];
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < pi[i].sizeY; y++)
                        {
                            for (int x = 0; x < pi[i].sizeX; x++)
                            {
                                red[y, x] = r[y, x - imageDependencies.left + start];
                                green[y, x] = g[y, x - imageDependencies.left + start];
                                blue[y, x] = b[y, x - imageDependencies.left + start];
                            }
                        }
                    }
                    pi[i].r = red;
                    pi[i].g = green;
                    pi[i].b = blue;
                }

                start += stepSize;
            }
            return pi;
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
                        r = null;
                        g = null;
                        b = null;
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
                        r = new byte[this.sizeY, this.sizeX];
                        g = new byte[this.sizeY, this.sizeX];
                        b = new byte[this.sizeY, this.sizeX];
                        grayscale = false;
                    }
                    if (subPart.positionX == 0)
                    {
                        for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                        {
                            for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                            {
                                r[i + subPart.positionY, j + subPart.positionX] = subPart.r[i, j];
                                g[i + subPart.positionY, j + subPart.positionX] = subPart.g[i, j];
                                b[i + subPart.positionY, j + subPart.positionX] = subPart.b[i, j];
                            }
                        }
                    }
                    else
                    {
                        for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                        {
                            for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                            {
                                r[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.r[i, j];
                                g[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.g[i, j];
                                b[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.b[i, j];
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