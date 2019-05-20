using System;
using System.Collections.Generic;
using System.Text;

namespace KMeansFilter
{
    class GrayCluster
    {
        int index;
        byte gray;

        int count;
        int graySum;

        public GrayCluster(int index, byte gray)
        {
            this.index = index;
            addPixel(gray);
        }

        public int getIndex()
        {
            return index;
        }

        public byte getGray()
        {
            return gray;
        }

        public void addPixel(byte gray)
        {
            graySum += gray;
            count++;
            this.gray = computeGray();
        }

        public void removePixel(byte gray)
        {
            graySum -= gray;
            count--;
            this.gray = computeGray();
        }

        private byte computeGray()
        {
            return (byte)(graySum / count);
        }
    }
}
