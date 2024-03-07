using System.Net;

namespace XPing365.Sdk.Core.Configurations;

/// <summary>
/// The HttpClientConfiguration class is used to provide a set of properties for various features of 
/// the <see cref="HttpClient"/> class and its policies. Normally these properties have the predefined defaults, however 
/// can also be changed accordingly to your requirements.
/// </summary>
public class HttpClientConfiguration
{
    /// <summary>
    /// The name of the HttpClient that does not perform any retry policies.
    /// </summary>
    /// <remarks>
    /// This constant can be used to create or resolve a named HttpClient in the DependencyInjection service collection.
    /// </remarks>
    public const string HttpClientWithNoRetryAndNoFollowRedirect = nameof(HttpClientWithNoRetryAndNoFollowRedirect);
    /// <summary>
    /// The name of the HttpClient that performs retry policies.
    /// </summary>
    /// <remarks>
    /// This constant can be used to create or resolve a named HttpClient in the DependencyInjection service collection.
    /// </remarks>
    public const string HttpClientWithRetryAndNoFollowRedirect = nameof(HttpClientWithRetryAndNoFollowRedirect);

    /// <summary>
    /// Gets or sets how long a connection can be in the pool to be considered reusable. Default is 1 minute.
    /// See the remarks on <see cref="SocketsHttpHandler.PooledConnectionLifetime"/>.
    /// </summary>
    public TimeSpan PooledConnectionLifetime { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the sleep durations to wait for on each retry. Default is 1[s] on 1st retry, 5[s] on 2nd retry and
    /// 10[s] on 3rd retry. See the remarks on <see cref="Microsoft.Extensions.Http.PolicyHttpMessageHandler"/>.
    /// </summary>
    public IEnumerable<TimeSpan> SleepDurations { get; set; } = new[]
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10)
    };

    /// <summary>
    /// Gets or sets the number of exceptions or handled results that are allowed before opening the circuit.
    /// See the remarks on <see cref="Microsoft.Extensions.Http.PolicyHttpMessageHandler"/>.
    /// </summary>
    public int HandledEventsAllowedBeforeBreaking { get; set; } = 3;

    /// <summary>
    /// Gets or sets the duration the circuit will stay open before resetting. Default is 30[s].
    /// See the remarks on <see cref="Microsoft.Extensions.Http.PolicyHttpMessageHandler"/>.
    /// </summary>
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets a type of decompression method used by the handler for automatic decompression of the HTTP content
    /// response. Default is All.
    /// </summary>
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.All;
}
