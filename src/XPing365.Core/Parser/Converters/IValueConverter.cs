namespace XPing365.Core.Parser.Converters
{
    public interface IValueConverter 
    {
        public object? Convert(string value, Type targetType);
    }
}
