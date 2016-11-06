using System;
using System.Collections.Generic;
using System.Text;

namespace ParametersSDK
{
    public class ParametersFloat : IParameters
    {
        public readonly float minValue;
        public readonly float maxValue;
        public readonly float defaultValue;

        private readonly string displayName;
        private readonly DisplayType displayType;
        private List<object> valuesList;

        public ParametersFloat(float minValue, float maxValue, float defaultValue, string displayName, DisplayType displayType)
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
            if (newValue.GetType() == typeof(float))
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
                                float val = float.Parse(value);
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
