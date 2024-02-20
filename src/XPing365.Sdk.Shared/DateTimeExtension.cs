namespace XPing365.Sdk.Shared;

internal static class DateTimeExtensions
{
    /// <summary>
    /// Formats a TimeSpan object into a string, using the appropriate units and rounding.
    /// </summary>
    /// <param name="time">The TimeSpan object to format.</param>
    /// <returns>A string representation of the TimeSpan object.</returns>
    public static string GetFormattedTime(this TimeSpan time)
    {
        if (time.TotalMinutes >= 1)
        {
            return $"{Math.Round(time.TotalSeconds, 2)} min";
        }
        if (time.TotalSeconds >= 1)
        {
            return $"{Math.Round(time.TotalSeconds, 2)} s";
        }

        return $"{Math.Round(time.TotalMilliseconds, 0)} ms";
    }
}
