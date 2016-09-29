using System;
using System.Collections.Generic;
using System.Text;
using ParametersSDK;
using ProcessingImageSDK;

namespace Plugins.Masks
{
    public interface IMask
    {
        //every mask must have a static method called getParametersList
        //public static List<IParameters> getParametersList()
        //which will return the constructor parameters in the exact order

        byte[,] mask(ProcessingImage inputImage);
    }
}
