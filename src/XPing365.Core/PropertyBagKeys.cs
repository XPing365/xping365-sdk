namespace XPing365.Core;

public static class PropertyBagKeys
{
    #region Network Information
    public readonly static PropertyBagKey IPAddress = new(nameof(IPAddress));
    public readonly static PropertyBagKey IPStatus = new(nameof(IPStatus));
    #endregion Network Information

    #region DNS Lookup
    public readonly static PropertyBagKey DnsResolvedIPAddresses = new(nameof(DnsResolvedIPAddresses));
    #endregion // DNS Lookup

    #region PING 
    /// <summary>
    /// Number of times the ping data can be forwarded.
    /// </summary>
    public readonly static PropertyBagKey PingTTLOption = new(nameof(PingTTLOption)); 
    public readonly static PropertyBagKey PingDontFragmetOption = new(nameof(PingDontFragmetOption));
    public readonly static PropertyBagKey PingRoundtripTime = new(nameof(PingRoundtripTime));
    #endregion

    #region Http Property Bag Keys
    public readonly static PropertyBagKey UserAgent = new(nameof(UserAgent));
    public readonly static PropertyBagKey HttpMethod = new(nameof(HttpMethod));
    public readonly static PropertyBagKey HttpHeaders = new(nameof(HttpHeaders));
    public readonly static PropertyBagKey HttpStatus = new(nameof(HttpStatus));
    public readonly static PropertyBagKey HttpReasonPhrase = new(nameof(HttpReasonPhrase));
    public readonly static PropertyBagKey HttpVersion = new(nameof(HttpVersion));
    public readonly static PropertyBagKey HttpContent = new(nameof(HttpContent));
    public readonly static PropertyBagKey HttpRetry = new(nameof(HttpRetry));
    public readonly static PropertyBagKey HttpFollowRedirect = new(nameof(HttpFollowRedirect));
    #endregion // Http Property Bag Keys
}
