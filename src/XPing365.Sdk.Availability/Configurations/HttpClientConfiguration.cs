namespace XPing365.Sdk.Availability.Configurations;

public class HttpClientConfiguration
{
    public TimeSpan PooledConnectionLifetime { get; set; } = TimeSpan.FromMinutes(1);

    public IEnumerable<TimeSpan> SleepDurations { get; set; } = new[]
    { 
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10)
    };
    public int HandledEventsAllowedBeforeBreaking { get; set; } = 3;
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);
}
