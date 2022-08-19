using System.Text.RegularExpressions;
using XPing365.Shared;

namespace XPing365.Core.Parameter.Internal
{
    internal class DefaultParameterSetBuilder : IParameterSetBuilder
    {
        private const string Pattern = "\\{[a-zA-Z]+\\}";
        private readonly string url;
        private readonly IParameterSet parameterSet;

        public DefaultParameterSetBuilder(string url, IParameterSet parameterSet)
        {
            this.url = url.RequireNotNull(nameof(url));
            this.parameterSet = parameterSet.RequireNotNull(nameof(parameterSet));
        }

        public IList<string> Build()
        {
            List<string> urls = new();

            if (this.url.ToLower().Contains(this.parameterSet.Name.ToLower()))
            {
                foreach (var query in this.parameterSet.RawValues)
                {
                    if (Regex.IsMatch(this.url, Pattern))
                    {
                        var result = Regex.Replace(this.url, Pattern, query);
                        urls.Add(result);
                    }
                }
            }

            return urls;
        }
    }
}
