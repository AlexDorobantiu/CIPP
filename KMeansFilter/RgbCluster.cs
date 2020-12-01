namespace KMeansFilter
{
    class RgbCluster
    {
        readonly int index;
        Rgb rgb;

        int count;
        int redSum;
        int greenSum;
        int blueSum;

        public RgbCluster(int index, Rgb rgb)
        {
            this.index = index;
            addPixel(rgb);
        }

        public int getIndex()
        {
            return index;
        }

        public Rgb getRgb()
        {
            return rgb;
        }

        public void addPixel(Rgb rgb)
        {
            redSum += rgb.r;
            greenSum += rgb.g;
            blueSum += rgb.b;
            count++;
            this.rgb = computeRgb();
        }

        public void removePixel(Rgb rgb)
        {
            redSum -= rgb.r;
            greenSum -= rgb.g;
            blueSum -= rgb.b;
            count--;
            this.rgb = computeRgb();
        }

        private Rgb computeRgb()
        {
            return new Rgb((byte)(redSum / count), (byte)(greenSum / count), (byte)(blueSum / count));
        }
    }
}
