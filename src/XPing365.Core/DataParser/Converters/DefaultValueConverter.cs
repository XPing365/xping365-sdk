namespace XPing365.Core.DataParser.Converters
{
    public class DefaultValueConverter : IValueConverter
    {
        public object? Convert(string? value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            value = value.Trim();

            if (targetType == typeof(string))
            {
                return value;
            }
            if (targetType == typeof(int))
            {
                return int.Parse(value);
            }
            if (targetType == typeof(float))
            {
                return float.Parse(value);
            }
            if (targetType == typeof(double))
            {
                return double.Parse(value);
            }
            if (targetType == typeof(bool))
            {
                if (bool.TryParse(value, out bool result))
                {
                    return result;                    
                }

                if (double.TryParse(value, out double number))
                {
                    return number != 0;
                }
            }

            return value;
        }
    }
}
