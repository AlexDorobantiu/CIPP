using System;
using System.Collections.Generic;
using System.Text;

namespace ParametersSDK
{
    public enum DisplayType
    {
        textBox,
        trackBar,
        checkBox,
        comboBox,
        listBox
    }

    public interface IParameters
    {
        string getDisplayName();
        DisplayType getPreferredDisplayType();
        List<Object> getValues();
        void updateProperty(Object newValue);
    }
}
