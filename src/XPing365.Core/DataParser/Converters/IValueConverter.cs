namespace XPing365.Core.DataParser.Converters
{
    public interface IValueConverter 
    {
        public object? Convert(string value, Type targetType);
    }
}
