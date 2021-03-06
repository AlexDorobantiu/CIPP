using System;
using System.Collections.Generic;

namespace ParametersSDK
{
    public class ParametersEnum : IParameters
    {
        public readonly int defaultSelected;
        public readonly string[] displayValues;
        private readonly string displayName;
        private readonly ParameterDisplayTypeEnum displayType;

        private readonly List<object> valuesList;

        public ParametersEnum(string displayName, int defaultSelected, string[] displayValues, ParameterDisplayTypeEnum displayType)
        {
            this.displayName = displayName;
            this.defaultSelected = defaultSelected;
            this.displayValues = displayValues;
            this.displayType = displayType;
            valuesList = new List<object>
            {
                defaultSelected
            };
        }

        #region IParameters Members

        public string getDisplayName()
        {
            return displayName;
        }

        public ParameterDisplayTypeEnum getPreferredDisplayType()
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
            {
                if (newValue.GetType() == typeof(int[]))
                {
                    foreach (int i in (int[])newValue)
                    {
                        valuesList.Add(i);
                    }
                }
            }
        }

        #endregion
    }
}
