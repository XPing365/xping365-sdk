using XPing365.Core.Parameter.Internal;
using XPing365.Shared;

namespace XPing365.Core.Parameter
{
    public class ParameterSet : IParameterSet
    {
        public string Name { get; }

        public IList<string> RawValues { get; }

        public ParameterSet(string name, IList<string> rawValues)
        {
            this.Name = name.RequireNotNullOrWhiteSpace(nameof(name))
                            .RequireCondition(s => !string.IsNullOrEmpty(s), nameof(name), $"Parameter {nameof(name)} is null or empty");
            this.RawValues = rawValues.RequireNotNull(nameof(rawValues))
                                      .RequireCondition(l => l.Count > 0, nameof(rawValues), $"Parameter {nameof(rawValues)} is empty");
        }

        IParameterSetBuilder IParameterSet.CreateBuilder(string url)
        {
            return new DefaultParameterSetBuilder(url.RequireNotNull(nameof(url)), this);
        }
    }
}
