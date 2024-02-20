using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using XPing365.Sdk.Core.Common;

namespace XPing365.Sdk.Core.Session.Serialization;

public sealed class TestSessionSerializer
{
    private readonly DataContractSerializer dataContractSerializer = new(
        type: typeof(TestSession),
        knownTypes: GetKnownTypes());

    public void Serialize(TestSession session, Stream stream, SerializationFormat format, bool ownsStream = false)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        using var writer = format switch
        {
            SerializationFormat.Binary => XmlDictionaryWriter.CreateBinaryWriter(
                stream, dictionary: null, session: null, ownsStream: false),
            SerializationFormat.XML => XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8, ownsStream),
            _ => throw new NotSupportedException("Only Binary or XML is supported as serialization format.")
        };

        dataContractSerializer.WriteObject(writer, session);
    }

    public TestSession? Deserialize(Stream stream, SerializationFormat format)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        using var reader = format switch
        {
            SerializationFormat.Binary => XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max),
            SerializationFormat.XML => XmlDictionaryReader.CreateTextReader(stream, XmlDictionaryReaderQuotas.Max),
            _ => throw new NotSupportedException("Only Binary or XML is supported as serialization format.")
        };

        var result = dataContractSerializer.ReadObject(reader, true);
        return result as TestSession;
    }

    private static List<Type> GetKnownTypes() => [
        typeof(TestStep[]),
            typeof(PropertyBag<IPropertyBagValue>),
            typeof(Dictionary<PropertyBagKey, IPropertyBagValue>),
            typeof(PropertyBagValue<byte[]>),
            typeof(PropertyBagValue<string>),
            typeof(PropertyBagValue<string[]>),
            typeof(PropertyBagValue<Dictionary<string, string>>)];
}
