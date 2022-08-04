namespace XPing365.Core.Parameter
{
    public interface IParameterSet
    {
        string Name { get; }

        IList<string> RawValues { get; }

        IParameterSetBuilder CreateBuilder(string url);
    }
}
