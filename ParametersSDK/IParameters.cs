using System.Collections.Generic;

namespace ParametersSDK
{
    public interface IParameters
    {
        string getDisplayName();
        ParameterDisplayTypeEnum getPreferredDisplayType();
        List<object> getValues();
        void updateProperty(object newValue);
    }
}
