using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessingImageSDK
{
    public class Motion
    {
        public int id;
        public int imageNumber;
        public int missingVectors;
        public int blockSize;
        public int searchDistance;
        public ProcessingImage[] imageList;
        public MotionVectorBase[][,] vectors;

        public Motion(int id, int blockSize, int searchDistance, List<ProcessingImage> processingImageList)
        {
            this.id = id;
            this.imageNumber = processingImageList.Count;
            this.missingVectors = imageNumber - 1;
            this.blockSize = blockSize;
            this.searchDistance = searchDistance;
            this.imageList = processingImageList.ToArray();
            this.vectors = new MotionVectorBase[missingVectors][,];
        }

        public void addMotionVectors(ProcessingImage image, MotionVectorBase[,] vectors)
        {
            for (int i = 0; i < imageNumber; i++)
                if (image == imageList[i])
                {
                    this.vectors[i] = vectors;
                    missingVectors--;
                    break;
                }
        }

        public void save(string fileName)
        {
        }
    }
}
