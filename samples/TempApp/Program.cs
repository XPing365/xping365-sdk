using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using XPing365.Sdk.Shared;

namespace TempApp;

public enum TestSessionState
{
    /// <summary>
    /// The session is still being created.
    /// </summary>
    [Display(Name = "not started")] NotStarted,
    /// <summary>
    /// The session has been completed. 
    /// </summary>
    [Display(Name = "completed")] Completed,
    /// <summary>
    /// The session has been declined by test agent.
    /// </summary>
    [Display(Name = "declined")] Declined,
}

[Serializable]
[KnownType(typeof(DnsResolvedIPAddressesBag))]
[KnownType(typeof(string[]))]
public sealed class DnsResolvedIPAddressesBag : ISerializable
{
    private readonly IPAddress[] _addresses;

    public ReadOnlyCollection<IPAddress> IPAddresses => _addresses.AsReadOnly();

    public DnsResolvedIPAddressesBag(IPAddress[] addresses)
    {
        _addresses = addresses.RequireNotNull(nameof(addresses));
    }

    public DnsResolvedIPAddressesBag(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info);

        string[] addresses = info.GetValue(nameof(IPAddresses), typeof(string[])) as string[] ?? [];
        _addresses = addresses.Select(IPAddress.Parse).ToArray();
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        string[] adresses = _addresses.Select(addr => addr.ToString()).ToArray();
        info.AddValue(nameof(IPAddresses), adresses, typeof(string[]));
    }
}


[Serializable]
public sealed class TestSession : ISerializable, IDeserializationCallback
{
    public required Uri Url { get; init; }
    public required DateTime StartDate { get; init; }
    public required PropertyBag<ISerializable> PropertyBag { get; init; }
    public TestSessionState State { get; set; } = TestSessionState.NotStarted;

    public TestSession()
    { }

    public TestSession(SerializationInfo info, StreamingContext context)
    {
        ArgumentNullException.ThrowIfNull(info, nameof(info));

        Url = (Uri)info.GetValue(nameof(Url), typeof(Uri)).RequireNotNull(nameof(Url));
        StartDate = (DateTime)info.GetValue(nameof(StartDate), typeof(DateTime)).RequireNotNull(nameof(StartDate));
        State = Enum.Parse<TestSessionState>(
            value: (string)info.GetValue(nameof(State), typeof(string)).RequireNotNull(nameof(State)));
        PropertyBag = (PropertyBag<ISerializable>)info.GetValue(
                name: nameof(PropertyBag),
                type: typeof(PropertyBag<ISerializable>))
            .RequireNotNull(nameof(PropertyBag));
    }

    public void Save(Stream stream)
    {
        using var textDictionaryWriter = XmlDictionaryWriter.CreateTextWriter(stream);
        Save(textDictionaryWriter);
    }

    public void Save(XmlDictionaryWriter writer)
    {
        var dataContractSerializer = new DataContractSerializer(
            type: typeof(TestSession), 
            knownTypes: GetKnownTypes());
        dataContractSerializer.WriteObject(writer, this);
    }

    public static TestSession? Load(Stream stream)
    {
        using var textDictionaryReader = XmlDictionaryReader.CreateTextReader(
           stream, XmlDictionaryReaderQuotas.Max);

        return Load(textDictionaryReader);
    }

    public static TestSession? Load(XmlDictionaryReader reader)
    {
        var dataContractSerializer = new DataContractSerializer(
            type: typeof(TestSession),
            knownTypes: GetKnownTypes());
        var result = dataContractSerializer.ReadObject(reader);

        return result as TestSession;
    }

    private static List<Type> GetKnownTypes() => [
        typeof(PropertyBag<ISerializable>),
        typeof(Dictionary<PropertyBagKey, ISerializable>)];

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(Url), Url, typeof(Uri));
        info.AddValue(nameof(StartDate), StartDate, typeof(DateTime));
        info.AddValue(nameof(State), State.ToString(), typeof(string));
        info.AddValue(nameof(PropertyBag), PropertyBag, typeof(PropertyBag<ISerializable>));
    }

    void IDeserializationCallback.OnDeserialization(object? sender)
    {
        if (!Uri.TryCreate(Url.ToString(), UriKind.Absolute, out _))
        {
            throw new SerializationException($"The Url parameter is not a valid Uri: {Url}");
        }
    }
}



internal sealed class Program
{
    static void Main(string[] args)
    {
        // Create a stream to write to
        using Stream stream = File.Open("dict.xml", FileMode.Open);

        //IPAddress address = IPAddress.Parse("127.0.0.1");

        //var propertyBag = new PropertyBag<ISerializable>();
        //propertyBag.AddOrUpdateProperty(
        //    key: new PropertyBagKey(nameof(DnsResolvedIPAddressesBag)),
        //    value: new DnsResolvedIPAddressesBag([address]));

        //var session = new TestSession
        //{
        //    StartDate = DateTime.UtcNow,
        //    Url = new Uri("http://test.com"),
        //    State = TestSessionState.Completed,
        //    PropertyBag = propertyBag
        //};

        TestSession ? session1 = TestSession.Load(stream);
        //session.Save(stream);
    }
}
