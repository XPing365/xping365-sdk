namespace XPing365.Core;

public record TestSettings
{
    public const int DefaultTimeoutInSeconds = 30;

    public PropertyBag PropertyBag { get; } = new();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(DefaultTimeoutInSeconds);
    public bool RetryTestFailures { get; set; } = true;
    public bool FollowRedirectionResponses { get; set; } = true;

    public static TestSettings DefaultForAvailability
    {
        get
        {
            var testSettings = new TestSettings();
            testSettings.PropertyBag.AddOrUpdateProperties(new Dictionary<PropertyBagKey, object>
            {
                { PropertyBagKeys.PingDontFragmetOption, true },
                { PropertyBagKeys.PingTTLOption, 64 }
            });

            return testSettings;
        }
    }
}
