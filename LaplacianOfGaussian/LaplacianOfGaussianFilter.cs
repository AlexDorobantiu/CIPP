
using ProcessingImageSDK;

namespace Plugins.Filters.LaplacianOfGaussian
{
    public class LaplacianOfGaussianFilter : IFilter
    {
        #region IFilter Members

        public ImageDependencies getImageDependencies()
        {
            return new ImageDependencies(2, 2, 2, 2);
        }

        public ProcessingImage filter(ProcessingImage inputImage)
        {
            int[,] f = new int[5, 5];
            f[0, 0] = f[0, 1] = f[0, 3] = f[0, 4] = 0;
            f[0, 2] = -1;
            f[1, 0] = f[1, 4] = 0;
            f[1, 1] = f[1, 3] = -1;
            f[1, 2] = -2;
            f[2, 0] = f[2, 4] = -1;
            f[2, 1] = f[2, 3] = -2;
            f[2, 2] = 16;
            f[3, 0] = f[3, 4] = 0;
            f[3, 1] = f[3, 3] = -1;
            f[3, 2] = -2;
            f[4, 0] = f[4, 1] = f[4, 3] = f[4, 4] = 0;
            f[4, 2] = -1;
            ProcessingImage outputImage = inputImage.mirroredMarginConvolution(f);
            outputImage.addWatermark("Laplacian of Gaussian Filter v1.0, Alex Dorobanțiu");
            return outputImage;
        }

        #endregion
    }
}
