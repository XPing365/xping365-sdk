using System.Net;
using System.Runtime.Serialization;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Shared;

namespace XPing365.Sdk.Availability.TestBags;

[Serializable]
[KnownType(typeof(Dictionary<string, string>))]
public sealed class HttpResponseMessageBag : ISerializable
{
    private readonly byte[] _data;

    public static PropertyBagKey Key => new(nameof(HttpResponseMessageBag));
    public bool IsSuccessStatusCode { get; }
    public HttpStatusCode StatusCode { get; }
    public IDictionary<string, string>? ResponseHeaders { get; }
    public IDictionary<string, string>? TrailingHeaders { get; }
    public IDictionary<string, string>? ContentHeaders { get; }
    public string? ReasonPhrase { get; }
    public Version Version { get; }
    public byte[] GetContent() => _data;

    public HttpResponseMessageBag(HttpResponseMessage httpResponse, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(httpResponse, nameof(httpResponse));
        ArgumentNullException.ThrowIfNull(content, nameof(content));

        IsSuccessStatusCode = httpResponse.IsSuccessStatusCode;
        StatusCode = httpResponse.StatusCode;
        ResponseHeaders = httpResponse.Headers.ToDictionary(h => h.Key, h => string.Join(";", h.Value));
        TrailingHeaders = httpResponse.TrailingHeaders.ToDictionary(h => h.Key, h => string.Join(";", h.Value));
        ContentHeaders = httpResponse.Content.Headers.ToDictionary(h => h.Key, h => string.Join(";", h.Value));
        ReasonPhrase = httpResponse.ReasonPhrase;
        Version = httpResponse.Version;
        _data = content;
    }

    public HttpResponseMessageBag(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info, nameof(info));

        IsSuccessStatusCode = (bool)info
            .GetValue(nameof(IsSuccessStatusCode), typeof(bool))
            .RequireNotNull(nameof(IsSuccessStatusCode));
        StatusCode = Enum.Parse<HttpStatusCode>(
            value: (string)info.GetValue(nameof(StatusCode), typeof(string)).RequireNotNull(nameof(StatusCode)), 
            ignoreCase: true);
        ResponseHeaders = (Dictionary<string, string>)info
            .GetValue(nameof(ResponseHeaders), typeof(Dictionary<string, string>))
            .RequireNotNull(nameof(ResponseHeaders));
        TrailingHeaders = (Dictionary<string, string>)info
            .GetValue(nameof(TrailingHeaders), typeof(Dictionary<string, string>))
            .RequireNotNull(nameof(TrailingHeaders));
        ContentHeaders = (Dictionary<string, string>)info
            .GetValue(nameof(ContentHeaders), typeof(Dictionary<string, string>))
            .RequireNotNull(nameof(ContentHeaders));
        ReasonPhrase = info.GetValue(nameof(ReasonPhrase), typeof(string)) as string;
        Version = Version.Parse(
            input: (string)info.GetValue(nameof(Version), typeof(string)).RequireNotNull(nameof(Version)));
        _data = (byte[])info.GetValue("Content", typeof(byte[])).RequireNotNull("Content");
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(IsSuccessStatusCode), IsSuccessStatusCode, typeof(bool));
        info.AddValue(nameof(StatusCode), StatusCode.ToString(), typeof(string));
        info.AddValue(nameof(ResponseHeaders), ResponseHeaders, typeof(Dictionary<string, string>));
        info.AddValue(nameof(TrailingHeaders), TrailingHeaders, typeof(Dictionary<string, string>));
        info.AddValue(nameof(ContentHeaders), ContentHeaders, typeof(Dictionary<string, string>));
        info.AddValue(nameof(ReasonPhrase), ReasonPhrase, typeof(string));
        info.AddValue(nameof(Version), Version.ToString(), typeof(string));
        info.AddValue("Content", _data, typeof(byte[]));
    }
}
