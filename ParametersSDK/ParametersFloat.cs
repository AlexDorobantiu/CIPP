using System.Collections.Generic;
using System.Globalization;

namespace ParametersSDK
{
    public class ParametersFloat : IParameters
    {
        public readonly float minValue;
        public readonly float maxValue;
        public readonly float defaultValue;

        private readonly string displayName;
        private readonly ParameterDisplayTypeEnum displayType;
        private readonly List<object> valuesList;

        public ParametersFloat(string displayName, float defaultValue, float minValue, float maxValue, ParameterDisplayTypeEnum displayType)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.defaultValue = defaultValue;
            this.displayName = displayName;
            this.displayType = displayType;
            valuesList = new List<object>
            {
                defaultValue
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
            {
                if (newValue.GetType() == typeof(string))
                {
                    string n = (string)newValue;
                    string[] values = n.Split(", ".ToCharArray()); // split for comma or empty space
                    foreach (string value in values)
                    {
                        try
                        {
                            if (!string.Empty.Equals(value))
                            {
                                float val = float.Parse(value, CultureInfo.InvariantCulture);
                                if (val < minValue) val = minValue;
                                if (val > maxValue) val = maxValue;
                                valuesList.Add(val);
                            }
                        }
                        catch { }
                    }
                }
            }
        }
        #endregion
    }
}
