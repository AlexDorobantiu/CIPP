using System;
using System.Collections.Generic;
using System.Text;

namespace ParametersSDK
{    
    public interface IParameters
    {
        string getDisplayName();
        ParameterDisplayTypeEnum getPreferredDisplayType();
        List<Object> getValues();
        void updateProperty(Object newValue);
    }
}
