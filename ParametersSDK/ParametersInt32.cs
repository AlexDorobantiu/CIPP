using System;
using System.Collections.Generic;
using System.Text;

namespace ParametersSDK
{
    public class ParametersInt32 : IParameters
    {
        public readonly int minValue;
        public readonly int maxValue;
        public readonly int defaultValue;

        private readonly string displayName;
        private readonly DisplayType displayType;
        private List<object> valuesList;

        public ParametersInt32(int minValue, int maxValue, int defaultValue, string displayName, DisplayType displayType)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.defaultValue = defaultValue;
            this.displayName = displayName;
            this.displayType = displayType;
            valuesList = new List<object>();
            valuesList.Add(defaultValue);
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
                valuesList.Add(defaultValue);
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
                if (newValue.GetType() == typeof(string))
                {
                    string n = (string)newValue;
                    string[] values = n.Split(" ".ToCharArray()); //split only for an empty space
                    foreach (string value in values)
                    {
                        try
                        {
                            if (!string.Empty.Equals(value))
                            {
                                int val = int.Parse(value);
                                if (val < minValue) val = minValue;
                                if (val > maxValue) val = maxValue;
                                valuesList.Add(val);
                            }
                        }
                        catch { }
                    }
                }
        }
        #endregion
    }
}
