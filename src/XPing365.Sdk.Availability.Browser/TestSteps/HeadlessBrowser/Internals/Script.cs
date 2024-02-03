using XPing365.Sdk.Common;

namespace XPing365.Sdk.Availability.TestSteps.HeadlessBrowser.Internals;

internal static class Scripts
{
    public static readonly Script LoadHtml = new("LoadHtml.js", new FileInfo("Scripts\\LoadHtml.js"));
}

internal sealed class Script(string name, FileInfo path)
{
    public string Name { get; init; } = name.RequireNotNullOrWhiteSpace(nameof(name));
    public FileInfo Path { get; init; } = path.RequireNotNull(nameof(path));
}
