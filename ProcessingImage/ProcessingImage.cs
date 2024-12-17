using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ProcessingImageSDK.PixelStructures;
using ProcessingImageSDK.Position;
using ProcessingImageSDK.Utils;

namespace ProcessingImageSDK
{
    /// <summary>
    /// ProcessingImageSDK.ProcessingImage is an object used for work with images inside CIPP, designed to provide basic functionality image processing
    /// </summary>
    [Serializable]
    public class ProcessingImage
    {
        private static readonly List<string> knownExtensionsList = new List<string>() { ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", ".ICO", ".EMF", ".EXIF", ".TIFF", ".TIF", ".WMF" };

        private int sizeX;
        private int sizeY;

        private byte[,] alpha;   // alpha value for masking (if any)
        private byte[,] red;       // red component
        private byte[,] green;       // green component
        private byte[,] blue;       // blue component
        private byte[,] gray;    // gray component (if any)

        /// <summary>
        /// Is true when the image only has the gray (and alpha) component.
        /// </summary>
        public bool grayscale;

        /// <summary>
        /// Is true when at least one of the alpha values is different than 255 (full opaque)
        /// </summary>
        public bool masked;

        [NonSerialized]
        private byte[,] luminance;       // luminance component

        private string path;
        private string name;

        private List<string> watermaks = new List<string>();

        // position in the original image (if this is a subpart)
        private Position2d position;

        private ImageDependencies imageDependencies;

        /// <summary>
        /// Base image processing class for CIPP
        /// </summary>
        public ProcessingImage()
        {
        }

        /// <summary>
        /// Initializer method for a new ProcessingImage.
        /// </summary>
        /// <param name="name">The name which appears in the GUI</param>
        /// <param name="sizeX">The width of the image</param>
        /// <param name="sizeY">The height of the image</param>
        /// <param name="createOpaqueAlphaChannel">If true, a new opaque alpha channel will be created</param>
        /// <param name="position">The virtual position of the image (useful when the image is subdivided)</param>
        public void initialize(string name, int sizeX, int sizeY, bool createOpaqueAlphaChannel = true, Position2d position = new Position2d())
        {
            this.name = name;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            if (createOpaqueAlphaChannel)
            {
                setAlpha(ProcessingImageUtils.createChannel(sizeX, sizeY, 255));
            }

            this.position = position;
        }

        /// <summary>
        /// Gets the image name
        /// </summary>
        /// <returns>The name to be displayed on screen</returns>
        public string getName()
        {
            return name;
        }

        /// <summary>
        /// Sets the image name
        /// </summary>
        /// <param name="name">The name to be displayed on screen</param>
        public void setName(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the width of the image
        /// </summary>
        /// <returns>The width of the image</returns>
        public int getSizeX()
        {
            return sizeX;
        }

        /// <summary>
        /// Gets the height of the image
        /// </summary>
        /// <returns>The heigth of the image</returns>
        public int getSizeY()
        {
            return sizeY;
        }

        /// <summary>
        /// Gets the position of the image in a parent image from which this image was split
        /// </summary>
        /// <returns>The position</returns>
        public Position2d getPosition()
        {
            return position;
        }

        /// <summary>
        /// The red channel
        /// </summary>
        /// <returns></returns>
        public byte[,] getRed()
        {
            return red;
        }

        /// <summary>
        /// The green channel
        /// </summary>
        /// <returns></returns>
        public byte[,] getGreen()
        {
            return green;
        }

        /// <summary>
        /// The blue channel
        /// </summary>
        /// <returns></returns>
        public byte[,] getBlue()
        {
            return blue;
        }

        /// <summary>
        /// The alpha channel
        /// </summary>
        /// <returns></returns>
        public byte[,] getAlpha()
        {
            return alpha;
        }

        /// <summary>
        /// The gray channel
        /// </summary>
        /// <returns>The gray channel. If it doesn't exist at the time of the call, it is computed and then returned.</returns>
        public byte[,] getGray()
        {
            if (gray == null)
            {
                computeGray();
            }
            return gray;
        }

        /// <summary>
        /// The luminance channel
        /// </summary>
        /// <returns>The luminance channel. If it doesn't exist at the time of the call, it is computed and then returned.</returns>
        public byte[,] getLuminance()
        {
            if (luminance == null)
            {
                computeLuminance();
            }
            return luminance;
        }

        /// <summary>
        /// Sets the width of the image
        /// </summary>
        /// <param name="sizeX"></param>
        public void setSizeX(int sizeX)
        {
            this.sizeX = sizeX;
        }

        /// <summary>
        /// Sets the height of the image
        /// </summary>
        /// <param name="sizeY"></param>
        public void setSizeY(int sizeY)
        {
            this.sizeY = sizeY;
        }

        /// <summary>
        /// Sets the red channel
        /// </summary>
        /// <param name="red"></param>
        public void setRed(byte[,] red)
        {
            this.red = red;
        }

        /// <summary>
        /// Sets the green channel
        /// </summary>
        /// <param name="green"></param>
        public void setGreen(byte[,] green)
        {
            this.green = green;
        }

        /// <summary>
        /// Sets the blue channel
        /// </summary>
        /// <param name="blue"></param>
        public void setBlue(byte[,] blue)
        {
            this.blue = blue;
        }

        /// <summary>
        /// Sets the alpha channel
        /// </summary>
        /// <param name="alpha"></param>
        public void setAlpha(byte[,] alpha)
        {
            this.alpha = alpha;
        }

        /// <summary>
        /// Returns the file system path of the ProcessingImage. If the ProcessingImage was not loaded from the disk, then it should be null.
        /// </summary>
        /// <returns></returns>
        public string getPath()
        {
            return path;
        }

        /// <summary>
        /// Sets the gray channel to the image and implicitly sets the image as grayscale and resets the other color channels
        /// </summary>
        /// <param name="gray"></param>
        public void setGray(byte[,] gray)
        {
            this.gray = gray;
            grayscale = true;
            red = null;
            green = null;
            blue = null;
            luminance = null;
        }

        /// <summary>
        /// Loads the content from a Bitmap object
        /// </summary>
        /// <param name="bitmap">The Bitmap object</param>
        public void loadImage(Bitmap bitmap)
        {
            sizeX = bitmap.Width;
            sizeY = bitmap.Height;
            position = new Position2d();

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
                    }
                    break;
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
                    }
                    break;
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
                    }
                    break;
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
                    }
                    break;
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
                    }
                    break;
                default: { } break;
            }

            if (grayscale)
            {
                gray = red;
                red = null;
                green = null;
                blue = null;
            }
        }

        /// <summary>
        /// Loads Image from the specified file name
        /// </summary>
        /// <param name="fileName">Full path of the file to be loaded</param>
        public void loadImage(string fileName)
        {
            Bitmap bitmap = new Bitmap(fileName);
            loadImage(bitmap);
            path = fileName;
            name = Path.GetFileName(fileName);
        }

        /// <summary>
        /// Saves Image to specified file name using extension filetype. If no extension is provided, image is saved to the default type PNG
        /// </summary>
        /// <param name="fileName">Full path of the file to be loaded</param>
        public void saveImage(string fileName)
        {
            Bitmap bitmap = grayscale ? getBitmap(ProcessingImageBitmapType.AlphaGray) : getBitmap(ProcessingImageBitmapType.AlphaColor);
            string extension = Path.GetExtension(fileName);
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

        /// <summary>
        /// Computes the grayscale channel from the red, green and blue components by averaging them
        /// </summary>
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
                    gray[i, j] = (byte)Math.Round((red[i, j] + green[i, j] + blue[i, j]) / 3.0f);
                }
            }
        }

        /// <summary>
        /// Compute the luminance channel from the red, green and blue components using an weighted average
        /// </summary>
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

        /// <summary>
        /// Creates a System.Drawing.Bitmap from the ProcessingImage.
        /// </summary>
        /// <param name="type">Specifies which channels to include when creating the bitmap</param>
        /// <returns>A new System.Drawing.Bitmap</returns>
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;

                        case ProcessingImageBitmapType.AlphaGray:
                            {
                                if (gray == null)
                                {
                                    computeGray();
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
                            }
                            break;
                        case ProcessingImageBitmapType.Gray:
                            {
                                if (gray == null)
                                {
                                    computeGray();
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
                            }
                            break;
                        case ProcessingImageBitmapType.AlphaLuminance:
                            {
                                if (luminance == null)
                                {
                                    computeLuminance();
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
                            }
                            break;
                        case ProcessingImageBitmapType.Luminance:
                            {
                                if (luminance == null)
                                {
                                    computeLuminance();
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
                            }
                            break;
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
                            }
                            break;
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
                            }
                            break;
                        case ProcessingImageBitmapType.AlphaGray:
                            {
                                if (gray == null)
                                {
                                    computeGray();
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
                            }
                            break;
                        default:
                            {
                                if (gray == null)
                                {
                                    computeGray();
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
                            }
                            break;
                    }
                }

                bitmap.UnlockBits(bitmapData);
                return bitmap;
            }
            catch (Exception e)
            {
                throw new Exception("Could not convert to bitmap.", e);
            }
        }

        /// <summary>
        /// Creates a System.Drawing.Bitmap for displaying scaled to the specified size, but keeping the aspect ratio.
        /// </summary>
        /// <param name="sizeX">The desired width</param>
        /// <param name="sizeY">The desired height</param>
        /// <returns>A new System.Drawing.Bitmap</returns>
        public Bitmap getPreviewBitmap(int sizeX, int sizeY)
        {
            if (sizeX == 0 || sizeY == 0)
            {
                return null;
            }
            try
            {
                if (alpha == null)
                {
                    setAlpha(ProcessingImageUtils.createChannel(this.sizeX, this.sizeY, 255));
                    watermaks.Add("Alpha channel was missing. A default one was generated.");
                }

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
            catch (Exception e)
            {
                throw new Exception("Could not convert to preview bitmap. " + e.Message);
            }
        }

        /// <summary>
        /// The list of image watermarks which contain the information of what processing was applied so far
        /// </summary>
        /// <returns>The list of watermarks</returns>
        public List<string> getWatermarks()
        {
            return watermaks;
        }

        /// <summary>
        /// Adds a watermark
        /// </summary>
        /// <param name="watermark"></param>
        public void addWatermark(string watermark)
        {
            watermaks.Add(watermark);
        }

        /// <summary>
        /// Clones the watermarks from the source
        /// </summary>
        /// <param name="source"></param>
        public void cloneWatermarks(ProcessingImage source)
        {
            watermaks.Clear();
            watermaks.AddRange(source.watermaks);
        }

        /// <summary>
        /// Copies to from the source to the target the following:
        ///  - The display name
        ///  - The width and the height
        ///  - The grayscale flag
        ///  - The masked flag
        ///  - The watermarks (if cloneWatermarks is set to true)
        ///  - The alpha channel (if cloneAlpha is set to true)
        ///  - The image dependencies (useful when using parallel processing)
        ///  - The position in a parent image (useful when using parallel processing)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="cloneWatermarks"></param>
        /// <param name="cloneAlpha"></param>
        public static void copyAttributes(ProcessingImage source, ProcessingImage target, bool cloneWatermarks = true, bool cloneAlpha = true)
        {
            target.grayscale = source.grayscale;
            target.masked = source.masked;
            target.sizeX = source.sizeX;
            target.sizeY = source.sizeY;
            target.position = source.position;
            target.imageDependencies = source.imageDependencies;

            target.path = null;
            target.name = source.name;

            if (cloneWatermarks)
            {
                target.cloneWatermarks(source);
            }
            if (cloneAlpha)
            {
                target.alpha = (byte[,])source.getAlpha().Clone();
            }
        }

        /// <summary>
        /// Copies to from the originalImage to the current image the following:
        ///  - The display name
        ///  - The width and the height
        ///  - The grayscale flag
        ///  - The masked flag
        ///  - The watermarks
        ///  - The image dependencies (useful when using parallel processing)
        ///  - The position in a parent image (useful when using parallel processing)
        /// </summary>
        /// <param name="originalImage"></param>
        public void copyAttributes(ProcessingImage originalImage)
        {
            copyAttributes(originalImage, this, true, false);
        }

        /// <summary>
        /// Copies to from the originalImage to the current image the following:
        ///  - The display name
        ///  - The width and the height
        ///  - The grayscale flag
        ///  - The masked flag
        ///  - The watermarks
        ///  - The alpha channel
        ///  - The image dependencies (useful when using parallel processing)
        ///  - The position in a parent image (useful when using parallel processing)
        /// </summary>
        /// <param name="originalImage"></param>
        public void copyAttributesAndAlpha(ProcessingImage originalImage)
        {
            copyAttributes(originalImage, this, true, true);
        }

        /// <summary>
        /// Creates a duplicate of the image with the same attributes as the current one
        /// </summary>
        /// <param name="cloneAlpha"></param>
        /// <returns></returns>
        public ProcessingImage clone(bool cloneAlpha = true)
        {
            ProcessingImage processingImage = new ProcessingImage();
            copyAttributes(this, processingImage, cloneAlpha);
            if (!grayscale)
            {
                processingImage.red = (byte[,])red.Clone();
                processingImage.green = (byte[,])green.Clone();
                processingImage.blue = (byte[,])blue.Clone();
            }
            else
            {
                processingImage.grayscale = true;
                processingImage.gray = (byte[,])gray.Clone();
            }
            return processingImage;
        }

        /// <summary>
        /// Creates a duplicate of the image with the same attributes as the current one except the alpha channel which is replaced by the specified one
        /// </summary>
        /// <param name="alphaChannel"></param>
        /// <returns></returns>
        public ProcessingImage cloneAndSubstituteAlpha(byte[,] alphaChannel)
        {
            ProcessingImage processingImage = clone(false);
            processingImage.alpha = alphaChannel;
            return processingImage;
        }

        /// <summary>
        /// Creates a duplicate of the image but with blank channels
        /// </summary>
        /// <returns></returns>
        public ProcessingImage blankClone()
        {
            ProcessingImage processingImage = new ProcessingImage();
            copyAttributes(this, processingImage, false);

            processingImage.alpha = ProcessingImageUtils.createChannel(sizeX, sizeY, 255);
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

        /// <summary>
        /// Computes an image which is the result of the convolution with the integer kernel specified
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Computes an image which is the result of the convolution with the floating point kernel specified
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Computes an image which is the result of the convolution with the integer kernel specified and which keeps the original image size by mirroring the margins when applying the kernel
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Computes an image which is the result of the convolution with the floating point kernel specified and which keeps the original image size by mirroring the margins when applying the kernel
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the list of supported filetypes for loading and saving
        /// </summary>
        /// <returns></returns>
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
            if (imageDependencies == null)
            {
                return null;
            }
            if (imageDependencies.left < 0 || imageDependencies.right < 0 || imageDependencies.top < 0 || imageDependencies.bottom < 0)
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

            ProcessingImage[] processingImages = new ProcessingImage[subParts];
            for (int i = 0; i < subParts; i++)
            {
                processingImages[i] = new ProcessingImage();
            }

            int stepSize = sizeX / subParts;
            int size = stepSize + imageDependencies.left + imageDependencies.right;
            int startX = 0;

            for (int i = 0; i < subParts; i++)
            {
                processingImages[i].imageDependencies = imageDependencies;
                processingImages[i].position.x = startX;
                processingImages[i].position.y = 0;
                if (i == 0)
                {
                    processingImages[i].sizeX = stepSize + imageDependencies.right;
                }
                else
                {
                    if (i == subParts - 1)
                    {
                        processingImages[i].sizeX = sizeX - (subParts - 1) * stepSize + imageDependencies.left;
                    }
                    else
                    {
                        processingImages[i].sizeX = size;
                    }
                }
                processingImages[i].sizeY = sizeY;


                byte[,] alpha = new byte[processingImages[i].sizeY, processingImages[i].sizeX];
                if (i == 0)
                {
                    for (int y = 0; y < processingImages[i].sizeY; y++)
                    {
                        for (int x = 0; x < processingImages[i].sizeX; x++)
                        {
                            alpha[y, x] = this.alpha[y, x];
                        }
                    }
                }
                else
                {
                    for (int y = 0; y < processingImages[i].sizeY; y++)
                    {
                        for (int x = 0; x < processingImages[i].sizeX; x++)
                        {
                            alpha[y, x] = this.alpha[y, x - imageDependencies.left + startX];
                        }
                    }
                }

                processingImages[i].alpha = alpha;

                if (grayscale)
                {
                    processingImages[i].grayscale = true;
                    byte[,] gray = new byte[processingImages[i].sizeY, processingImages[i].sizeX];
                    if (i == 0)
                    {
                        for (int y = 0; y < processingImages[i].sizeY; y++)
                        {
                            for (int x = 0; x < processingImages[i].sizeX; x++)
                            {
                                gray[y, x] = this.gray[y, x];
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < processingImages[i].sizeY; y++)
                        {
                            for (int x = 0; x < processingImages[i].sizeX; x++)
                            {
                                gray[y, x] = this.gray[y, x - imageDependencies.left + startX];
                            }
                        }
                    }
                    processingImages[i].gray = gray;
                }
                else
                {
                    byte[,] red = new byte[processingImages[i].sizeY, processingImages[i].sizeX];
                    byte[,] green = new byte[processingImages[i].sizeY, processingImages[i].sizeX];
                    byte[,] blue = new byte[processingImages[i].sizeY, processingImages[i].sizeX];

                    if (i == 0)
                    {
                        for (int y = 0; y < processingImages[i].sizeY; y++)
                        {
                            for (int x = 0; x < processingImages[i].sizeX; x++)
                            {
                                red[y, x] = this.red[y, x];
                                green[y, x] = this.green[y, x];
                                blue[y, x] = this.blue[y, x];
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < processingImages[i].sizeY; y++)
                        {
                            for (int x = 0; x < processingImages[i].sizeX; x++)
                            {
                                red[y, x] = this.red[y, x - imageDependencies.left + startX];
                                green[y, x] = this.green[y, x - imageDependencies.left + startX];
                                blue[y, x] = this.blue[y, x - imageDependencies.left + startX];
                            }
                        }
                    }
                    processingImages[i].red = red;
                    processingImages[i].green = green;
                    processingImages[i].blue = blue;
                }

                startX += stepSize;
            }
            return processingImages;
        }

        /// <summary>
        /// Attaches an image to the current one at the subpart position
        /// </summary>
        /// <param name="subPart"></param>
        public void join(ProcessingImage subPart)
        {
            try
            {
                int subPartStartX = subPart.imageDependencies.left;
                int subPartEndX = subPart.sizeX - subPart.imageDependencies.right;
                if (subPart.position.x == 0)
                {
                    watermaks = subPart.watermaks; // copy watermarks only from the first chunk
                    subPartStartX = 0;
                }
                else
                {
                    if (subPart.position.x + subPart.sizeX >= sizeX)
                    {
                        subPartEndX = subPart.sizeX;
                    }
                }

                for (int i = 0; i < subPart.sizeY; i++)
                {
                    for (int j = subPartStartX; j < subPartEndX; j++)
                    {
                        alpha[i + subPart.position.y, j + subPart.position.x - subPartStartX] = subPart.alpha[i, j];
                    }
                }

                if (subPart.grayscale)
                {
                    if (!grayscale)
                    {
                        red = null;
                        green = null;
                        blue = null;
                        gray = new byte[sizeY, sizeX];
                        grayscale = true;
                    }
                    for (int i = 0; i < subPart.sizeY; i++)
                    {
                        for (int j = subPartStartX; j < subPartEndX; j++)
                        {
                            gray[i + subPart.position.y, j + subPart.position.x - subPartStartX] = subPart.gray[i, j];
                        }
                    }
                }
                else
                {
                    if (grayscale)
                    {
                        gray = null;
                        red = new byte[sizeY, sizeX];
                        green = new byte[sizeY, sizeX];
                        blue = new byte[sizeY, sizeX];
                        grayscale = false;
                    }
                    for (int i = 0; i < subPart.sizeY; i++)
                    {
                        for (int j = subPartStartX; j < subPartEndX; j++)
                        {
                            red[i + subPart.position.y, j + subPart.position.x - subPartStartX] = subPart.red[i, j];
                            green[i + subPart.position.y, j + subPart.position.x - subPartStartX] = subPart.green[i, j];
                            blue[i + subPart.position.y, j + subPart.position.x - subPartStartX] = subPart.blue[i, j];
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Image join failed");
            }
        }

        /// <summary>
        /// Returns the display name of the ProcessingImage
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return name;
        }

    }
}