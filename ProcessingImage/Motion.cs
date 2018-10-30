using System;
using System.Collections.Generic;
using System.Text;
using ProcessingImageSDK.MotionVectors;

namespace ProcessingImageSDK
{
    public class Motion
    {
        public readonly int id;
        public readonly int imageNumber;
        public readonly int blockSize;
        public readonly int searchDistance;
        public readonly ProcessingImage[] imageList;

        public int missingVectors;
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
            {
                if (image == imageList[i])
                {
                    this.vectors[i] = vectors;
                    missingVectors--;
                    break;
                }
            }
        }
    }
}
