using System;
using System.Collections.Generic;
using System.Text;

namespace ParametersSDK
{
    public class ParametersEnum : IParameters
    {
        public readonly int defaultSelected;
        public readonly String[] displayValues;
        private readonly string displayName;
        private readonly DisplayType displayType;

        private List<object> valuesList;

        public ParametersEnum(string displayName, int defaultSelected, String[] displayValues, DisplayType displayType)
        {
            this.displayName = displayName;
            this.defaultSelected = defaultSelected;
            this.displayValues = displayValues;
            this.displayType = displayType;
            valuesList = new List<object>();
            valuesList.Add(defaultSelected);
        }

        #region IParameters Members

        public string getDisplayName()
        {
            return displayName;
        }

        public DisplayType getPreferredDisplayType()
        {
            return displayType;
        }

        public List<object> getValues()
        {
            if (valuesList.Count == 0)
            {
                valuesList.Add(defaultSelected);
            }
            return valuesList;
        }

        public void updateProperty(object newValue)
        {
            valuesList.Clear();
            if (newValue.GetType() == typeof(int))
            {
                valuesList.Add(newValue);
            }
            else
                if (newValue.GetType() == typeof(int[]))
                {
                    foreach (int i in (int[])newValue)
                    {
                        valuesList.Add(i);
                    }
                }
        }

        #endregion
    }
}
