using ProcessingImageSDK;

namespace Plugins.Filters
{
    public interface IFilter
    {
        // every filter must have a static method called getParametersList
        // public static List<IParameters> getParametersList()
        // which will return the constructor parameters in the exact order

        // if one cannot tell, return null
        ImageDependencies getImageDependencies();

        ProcessingImage filter(ProcessingImage inputImage);
    }
}
