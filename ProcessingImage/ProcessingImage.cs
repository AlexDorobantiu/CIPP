using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;

namespace ProcessingImageSDK
{
    public enum BitmapType
    {
        Alpha,
        AlphaColor,
        AlphaRed,
        AlphaGreen,
        AlphaBlue,
        AlphaRedGreen,
        AlphaRedBlue,
        AlphaGreenBlue,
        Color,
        Red,
        Green,
        Blue,
        RedGreen,
        RedBlue,
        GreenBlue,
        AlphaGray,
        Gray,
        AlphaLuminance,
        Luminance
    }
    struct Pixel32Bpp
    {
        public byte blue, green, red, alpha;
    }
    struct Pixel24Bpp
    {
        public byte blue, green, red;
    }
    struct Pixel8Bpp
    {
        public byte gray;
    }

    [Serializable]
    public struct ImageDependencies
    {
        public int left;
        public int right;
        public int top;
        public int bottom;

        public ImageDependencies(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
    }

    // Summary:
    //     ProcessingImageSDK.ProcessingImage is an object used to work with images
    //     inside CIPP, designed to be a flexible format for image processing
    [Serializable]
    public class ProcessingImage
    {
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
        private byte[,] y;       //luminance component

        private string path;
        private string name;

        private List<string> watermaks = new List<string>();

        /// <summary>
        /// Base image processing class for CIPP
        /// </summary>
        public ProcessingImage()
        {
        }

        public string getName() { return name; }
        public void setName(string newName) { name = newName; }

        public int getSizeX() { return sizeX; }
        public int getSizeY() { return sizeY; }
        public int getPositionX() { return positionX; }
        public int getPositionY() { return positionY; }
        public byte[,] getRed() { return r; }
        public byte[,] getGreen() { return g; }
        public byte[,] getBlue() { return b; }
        public byte[,] getAlpha() { return alpha; }
        public byte[,] getGray()
        {
            if (gray == null) computeGray();
            return gray;
        }
        public byte[,] getLuminance()
        {
            if (y == null) computeLuminance();
            return y;
        }
        public Color getPixel(int x, int y)
        {
            try
            {
                if (grayscale)
                    return Color.FromArgb(alpha[y, x], gray[y, x], gray[y, x], gray[y, x]);

                return Color.FromArgb(alpha[y, x], r[y, x], g[y, x], b[y, x]);
            }
            catch
            {
                return Color.Black;
            }
        }

        public void setSizeX(int sizeX) { this.sizeX = sizeX; }
        public void setSizeY(int sizeY) { this.sizeY = sizeY; }
        public void setRed(byte[,] red) { this.r = red; }
        public void setGreen(byte[,] green) { this.g = green; }
        public void setBlue(byte[,] blue) { this.b = blue; }
        public void setAlpha(byte[,] alpha) { this.alpha = alpha; }
        public void setGray(byte[,] gray)
        {
            this.gray = gray;
            this.grayscale = true;
            this.r = null;
            this.g = null;
            this.b = null;
        }
        public void setLuminance(byte[,] luminance) { this.y = luminance; }

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
            this.name = fileName.Substring(fileName.LastIndexOf('\\') + 1);
        }

        /// <summary>
        /// Saves Image to specified file name using extension filetype. If no extension is provided, image is saved to the default type PNG
        /// </summary>
        /// <param name="fileName">Full path of the file to be loaded</param>
        public void saveImage(string fileName)
        {
            Bitmap b = grayscale ? getBitmap(BitmapType.AlphaGray) : getBitmap(BitmapType.AlphaColor);
            String extension = fileName.Substring(fileName.LastIndexOf('.'));
            switch (extension)
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
            try
            {
                if (gray == null) gray = new byte[sizeY, sizeX];
                for (int i = 0; i < sizeY; i++)
                    for (int j = 0; j < sizeX; j++)
                    {
                        gray[i, j] = (byte)((r[i, j] + g[i, j] + b[i, j]) / 3);
                    }
            }
            catch
            {
            }
        }

        public void computeLuminance()
        {
            try
            {
                if (!grayscale)
                {
                    if (y == null) y = new byte[sizeY, sizeX];
                    for (int i = 0; i < sizeY; i++)
                        for (int j = 0; j < sizeX; j++)
                        {
                            y[i, j] = (byte)(r[i, j] * 0.3 + g[i, j] * 0.59 + b[i, j] * 0.11);
                        }
                }
                else
                {
                    y = (byte[,])(gray.Clone());
                }
            }
            catch
            {
            }
        }

        public Bitmap getBitmap(BitmapType type)
        {
            try
            {
                if (sizeX == 0 || sizeY == 0) return null;
                Bitmap bitmap = new Bitmap(sizeX, sizeY, PixelFormat.Format32bppArgb);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, sizeX, sizeY), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                if (!grayscale)
                {
                    switch (type)
                    {
                        case BitmapType.Alpha:
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
                        case BitmapType.AlphaColor:
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
                        case BitmapType.AlphaRed:
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
                        case BitmapType.AlphaGreen:
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
                        case BitmapType.AlphaBlue:
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
                        case BitmapType.AlphaRedGreen:
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
                        case BitmapType.AlphaRedBlue:
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
                        case BitmapType.AlphaGreenBlue:
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
                        case BitmapType.Red:
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
                        case BitmapType.Green:
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
                        case BitmapType.Blue:
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
                        case BitmapType.RedGreen:
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
                        case BitmapType.RedBlue:
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
                        case BitmapType.GreenBlue:
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

                        case BitmapType.AlphaGray:
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
                        case BitmapType.Gray:
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
                        case BitmapType.AlphaLuminance:
                            {
                                if (y == null) this.computeLuminance();
                                unsafe
                                {
                                    Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                    for (int i = 0; i < sizeY; i++)
                                    {
                                        for (int j = 0; j < sizeX; j++)
                                        {
                                            pBase->alpha = alpha[i, j];
                                            pBase->red = y[i, j];
                                            pBase->green = y[i, j];
                                            pBase->blue = y[i, j];
                                            pBase++;
                                        }
                                    }
                                }
                            } break;
                        case BitmapType.Luminance:
                            {
                                if (y == null) this.computeLuminance();
                                unsafe
                                {
                                    Pixel32Bpp* pBase = (Pixel32Bpp*)bitmapData.Scan0;
                                    for (int i = 0; i < sizeY; i++)
                                    {
                                        for (int j = 0; j < sizeX; j++)
                                        {
                                            pBase->alpha = 255;
                                            pBase->red = y[i, j];
                                            pBase->green = y[i, j];
                                            pBase->blue = y[i, j];
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
                        case BitmapType.Alpha:
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
                        case BitmapType.AlphaGray:
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
                return null;
            }
        }

        public Bitmap getPreviewBitmap(int sizeX, int sizeY)
        {
            try
            {
                //luam raportul cel mai mare
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
                return null;
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

        public void copyAttributes(ProcessingImage originalImage)
        {
            this.grayscale = originalImage.grayscale;
            this.masked = originalImage.masked;
            this.sizeX = originalImage.sizeX;
            this.sizeY = originalImage.sizeY;
            this.positionX = originalImage.positionX;
            this.positionY = originalImage.positionY;
            this.imageDependencies = originalImage.imageDependencies;

            this.path = null;
            this.name = originalImage.name;

            this.watermaks = new List<string>();
            watermaks.AddRange(originalImage.watermaks);
        }

        public void copyAttributesAndAlpha(ProcessingImage originalImage)
        {
            copyAttributes(originalImage);
            alpha = (byte[,])originalImage.getAlpha().Clone();
        }

        public ProcessingImage Clone()
        {
            ProcessingImage pi = new ProcessingImage();
            pi.sizeX = sizeX;
            pi.sizeY = sizeY;
            pi.positionX = positionX;
            pi.positionY = positionY;
            pi.imageDependencies = imageDependencies;

            pi.masked = this.masked;
            pi.name = this.name;
            pi.watermaks = new List<string>();
            foreach (string s in watermaks) pi.watermaks.Add(s);

            pi.alpha = (byte[,])this.alpha.Clone();
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

        public ProcessingImage alphaClone(byte[,] alphaChannel)
        {
            ProcessingImage pi = new ProcessingImage();
            pi.sizeX = sizeX;
            pi.sizeY = sizeY;
            pi.positionX = positionX;
            pi.positionY = positionY;
            pi.imageDependencies = imageDependencies;

            pi.masked = true;
            pi.name = this.name;
            pi.watermaks = new List<string>();
            foreach (string s in watermaks) pi.watermaks.Add(s);

            pi.alpha = alphaChannel;
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

        public ProcessingImage blankClone()
        {
            ProcessingImage pi = new ProcessingImage();
            pi.sizeX = sizeX;
            pi.sizeY = sizeY;
            pi.positionX = positionX;
            pi.positionY = positionY;
            pi.imageDependencies = imageDependencies;

            pi.masked = true;
            pi.name = this.name;
            pi.watermaks = new List<string>();
            foreach (string s in watermaks) pi.watermaks.Add(s);

            pi.alpha = new byte[sizeY, sizeX];
            for (int i = 0; i < sizeY; i++)
                for (int j = 0; j < sizeX; j++)
                    pi.alpha[i, j] = 255;

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
            pi.copyAttributes(this);
            pi.alpha = (byte[,])this.alpha.Clone();

            int filterX = matrix.GetLength(1);
            int filterY = matrix.GetLength(0);

            if (grayscale)
            {
                byte[,] g = new byte[sizeY, sizeX];
                for (int y = filterY - 1; y < sizeY; y++)
                {
                    for (int x = filterX - 1; x < sizeX; x++)
                    {
                        int sum = 0;
                        for (int i = filterY - 1; i >= 0; i--)
                            for (int j = filterX - 1; j >= 0; j--)
                                sum += matrix[i, j] * gray[y - i, x - j];
                        if (sum < 0) sum = 0;
                        if (sum > 255) sum = 255;
                        g[y, x] = (byte)sum;
                    }
                }
                pi.gray = g;
            }
            else
            {
                byte[,] red = new byte[sizeY, sizeX];
                byte[,] green = new byte[sizeY, sizeX];
                byte[,] blue = new byte[sizeY, sizeX];
                for (int y = filterY - 1; y < sizeY; y++)
                {
                    for (int x = filterX - 1; x < sizeX; x++)
                    {
                        int sumR = 0;
                        int sumG = 0;
                        int sumB = 0;
                        for (int i = filterY - 1; i >= 0; i--)
                            for (int j = filterX - 1; j >= 0; j--)
                            {
                                sumR += matrix[i, j] * r[y - i, x - j];
                                sumG += matrix[i, j] * g[y - i, x - j];
                                sumB += matrix[i, j] * b[y - i, x - j];
                            }
                        if (sumR < 0) sumR = 0;
                        if (sumR > 255) sumR = 255;
                        if (sumG < 0) sumG = 0;
                        if (sumG > 255) sumG = 255;
                        if (sumB < 0) sumB = 0;
                        if (sumB > 255) sumB = 255;
                        red[y, x] = (byte)sumR;
                        green[y, x] = (byte)sumG;
                        blue[y, x] = (byte)sumB;
                    }
                }
                pi.r = red;
                pi.g = green;
                pi.b = blue;
            }

            return pi;
        }

        public ProcessingImage convolution(float[,] matrix)
        {
            ProcessingImage pi = new ProcessingImage();
            pi.copyAttributes(this);
            pi.alpha = (byte[,])this.alpha.Clone();

            int filterX = matrix.GetLength(1);
            int filterY = matrix.GetLength(0);

            if (grayscale)
            {
                byte[,] g = new byte[sizeY, sizeX];
                for (int y = filterY - 1; y < sizeY; y++)
                {
                    for (int x = filterX - 1; x < sizeX; x++)
                    {
                        float sum = 0;
                        for (int i = filterY - 1; i >= 0; i--)
                            for (int j = filterX - 1; j >= 0; j--)
                                sum += matrix[i, j] * gray[y - i, x - j];
                        if (sum < 0) sum = 0;
                        if (sum > 255) sum = 255;
                        g[y, x] = (byte)sum;
                    }
                }
                pi.gray = g;
            }
            else
            {
                byte[,] red = new byte[sizeY, sizeX];
                byte[,] green = new byte[sizeY, sizeX];
                byte[,] blue = new byte[sizeY, sizeX];
                for (int y = filterY - 1; y < sizeY; y++)
                {
                    for (int x = filterX - 1; x < sizeX; x++)
                    {
                        float sumR = 0;
                        float sumG = 0;
                        float sumB = 0;
                        for (int i = filterY - 1; i >= 0; i--)
                            for (int j = filterX - 1; j >= 0; j--)
                            {
                                sumR += matrix[i, j] * r[y - i, x - j];
                                sumG += matrix[i, j] * g[y - i, x - j];
                                sumB += matrix[i, j] * b[y - i, x - j];
                            }
                        if (sumR < 0) sumR = 0;
                        if (sumR > 255) sumR = 255;
                        if (sumG < 0) sumG = 0;
                        if (sumG > 255) sumG = 255;
                        if (sumB < 0) sumB = 0;
                        if (sumB > 255) sumB = 255;
                        red[y, x] = (byte)sumR;
                        green[y, x] = (byte)sumG;
                        blue[y, x] = (byte)sumB;
                    }
                }
                pi.r = red;
                pi.g = green;
                pi.b = blue;
            }

            return pi;
        }

        public static List<string> getKnownExtensions()
        {
            List<string> list = new List<string>();
            list.Add(".PNG");
            list.Add(".JPG");
            list.Add(".JPEG");
            list.Add(".BMP");
            list.Add(".GIF");
            list.Add(".ICO");
            list.Add(".EMF");
            list.Add(".EXIF");
            list.Add(".TIFF");
            list.Add(".TIF");
            list.Add(".WMF");
            return list;
        }

        /// <summary>
        /// Creates an array of images
        /// </summary>
        /// <param name="imageDependencies">defines image dependencies</param>
        /// <param name="subParts">number of subparts to divide</param>
        /// <returns></returns>
        public ProcessingImage[] split(ImageDependencies imageDependencies, int subParts)
        {
            if (imageDependencies.left == -1) return null;
            if ((imageDependencies.left + imageDependencies.right) * subParts >= sizeX) return null;
            if ((imageDependencies.top + imageDependencies.bottom) * subParts >= sizeY) return null;

            ProcessingImage[] pi = new ProcessingImage[subParts];
            for (int i = 0; i < subParts; i++) pi[i] = new ProcessingImage();


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
                        for (int x = 0; x < pi[i].sizeX; x++)
                            al[y, x] = alpha[y, x];
                }
                else
                {
                    for (int y = 0; y < pi[i].sizeY; y++)
                        for (int x = 0; x < pi[i].sizeX; x++)
                            al[y, x] = alpha[y, x - imageDependencies.left + start];
                }

                pi[i].alpha = al;

                if (grayscale)
                {
                    pi[i].grayscale = true;
                    byte[,] gr = new byte[pi[i].sizeY, pi[i].sizeX];
                    if (i == 0)
                    {
                        for (int y = 0; y < pi[i].sizeY; y++)
                            for (int x = 0; x < pi[i].sizeX; x++)
                                gr[y, x] = gray[y, x];
                    }
                    else
                    {
                        for (int y = 0; y < pi[i].sizeY; y++)
                            for (int x = 0; x < pi[i].sizeX; x++)
                                gr[y, x] = gray[y, x - imageDependencies.left + start];
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
                            for (int x = 0; x < pi[i].sizeX; x++)
                            {
                                red[y, x] = r[y, x];
                                green[y, x] = g[y, x];
                                blue[y, x] = b[y, x];
                            }
                    }
                    else
                    {
                        for (int y = 0; y < pi[i].sizeY; y++)
                            for (int x = 0; x < pi[i].sizeX; x++)
                            {
                                red[y, x] = r[y, x - imageDependencies.left + start];
                                green[y, x] = g[y, x - imageDependencies.left + start];
                                blue[y, x] = b[y, x - imageDependencies.left + start];
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
                        for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                            alpha[i + subPart.positionY, j + subPart.positionX] = subPart.alpha[i, j];

                    this.watermaks = subPart.watermaks;
                }
                else
                {
                    for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                        for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                            alpha[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.alpha[i, j];
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
                            for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                                gray[i + subPart.positionY, j + subPart.positionX] = subPart.gray[i, j];
                    }
                    else
                    {
                        for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                            for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                                gray[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.gray[i, j];
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
                            for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                            {
                                r[i + subPart.positionY, j + subPart.positionX] = subPart.r[i, j];
                                g[i + subPart.positionY, j + subPart.positionX] = subPart.g[i, j];
                                b[i + subPart.positionY, j + subPart.positionX] = subPart.b[i, j];
                            }
                    }
                    else
                    {
                        for (int i = subPart.imageDependencies.top; i < subPart.sizeY - subPart.imageDependencies.bottom; i++)
                            for (int j = subPart.imageDependencies.left; j < subPart.sizeX - subPart.imageDependencies.right; j++)
                            {
                                r[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.r[i, j];
                                g[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.g[i, j];
                                b[i + subPart.positionY, j + subPart.positionX - subPart.imageDependencies.left] = subPart.b[i, j];
                            }
                    }
                }
            }
            catch
            {
            }
        }

        public override string ToString() { return name; }

        #region ISerializable Members

        ////Deserialization constructor.
        //public ProcessingImage(SerializationInfo info, StreamingContext ctxt)
        //{
        //    sizeX = (int)info.GetValue("sizeX", typeof(int));
        //    sizeY = (int)info.GetValue("sizeY", typeof(int));
        //    isMasked = (bool)info.GetValue("isMasked", typeof(bool));
        //    if (isMasked)
        //    {
        //        alpha = (byte[,])info.GetValue("alpha", typeof(byte[,]));
        //    }
        //    else
        //    {
        //        alpha = new byte[sizeY, sizeX];
        //        for (int i = 0; i < sizeY; i++)
        //            for (int j = 0; j < sizeX; j++)
        //                alpha[i, j] = 255;
        //    }
        //    isGrayscale = (bool)info.GetValue("isGrayscale", typeof(bool));
        //    if (isGrayscale)
        //    {
        //        alpha = (byte[,])info.GetValue("gray", typeof(byte[,]));
        //    }
        //    else
        //    {
        //        r = (byte[,])info.GetValue("red", typeof(byte[,]));
        //        g = (byte[,])info.GetValue("green", typeof(byte[,]));
        //        b = (byte[,])info.GetValue("blue", typeof(byte[,]));
        //    }
        //    watermaks = (List<string>)info.GetValue("watermarks", typeof(List<string>));
        //}

        ////Serialization function.
        //public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        //{
        //    info.AddValue("sizeX", sizeX);
        //    info.AddValue("sizeY", sizeY);
        //    info.AddValue("isMasked", isMasked);
        //    if (isMasked)
        //    {
        //        info.AddValue("alpha", alpha);
        //    }
        //    info.AddValue("isGrayscale", isGrayscale);
        //    if (isGrayscale)
        //    {
        //        info.AddValue("gray", gray);
        //    }
        //    else
        //    {
        //        info.AddValue("red", r);
        //        info.AddValue("green", g);
        //        info.AddValue("blue", b);
        //    }
        //    info.AddValue("watermarks", watermaks);
        //}

        #endregion
    }
}