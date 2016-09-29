using System;
using System.Collections.Generic;
using System.Text;

using ParametersSDK;
using ProcessingImageSDK;

namespace Plugins.MotionRecognition
{
    public interface IMotionRecognition
    {
        //every plugin must have a static method called getParametersList
        //public static List<IParameters> getParametersList()
        //which will return the constructor parameters in the exact order
        //the first two parameters must be blockSize and searchDistance defined as integers

        MotionVectorBase[,] scan(ProcessingImage frame, ProcessingImage nextFrame);        
    }
}
