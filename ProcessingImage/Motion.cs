using System.Collections.Generic;
using ProcessingImageSDK.MotionVectors;

namespace ProcessingImageSDK
{
    /// <summary>
    /// Models a Motion Detection result
    /// </summary>
    public class Motion
    {
        /// <summary>
        /// Unique id
        /// </summary>
        public readonly int id;

        /// <summary>
        /// Number of images as source for motion tracking
        /// </summary>
        public readonly int imageNumber;

        /// <summary>
        /// Size of the block used for block matching
        /// </summary>
        public readonly int blockSize;

        /// <summary>
        /// Maximum search distance around the current block
        /// </summary>
        public readonly int searchDistance;

        /// <summary>
        /// Source images
        /// </summary>
        public readonly ProcessingImage[] imageList;

        /// <summary>
        /// Number of still missing computed motion vectors
        /// </summary>
        public int missingVectors;

        /// <summary>
        /// Computed motion vectors for each block in each image
        /// </summary>
        public MotionVectorBase[][,] vectors;

        /// <summary>
        /// Constructor with initalizing of fields
        /// </summary>
        /// <param name="id"></param>
        /// <param name="blockSize"></param>
        /// <param name="searchDistance"></param>
        /// <param name="processingImageList"></param>
        public Motion(int id, int blockSize, int searchDistance, List<ProcessingImage> processingImageList)
        {
            this.id = id;
            imageNumber = processingImageList.Count;
            missingVectors = imageNumber - 1;
            this.blockSize = blockSize;
            this.searchDistance = searchDistance;
            imageList = processingImageList.ToArray();
            vectors = new MotionVectorBase[missingVectors][,];
        }

        /// <summary>
        /// Called when the motion vectors of one image were computed
        /// </summary>
        /// <param name="image">Image whose motion vectors were computed</param>
        /// <param name="vectors">Computed matrix of motion vectors</param>
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
