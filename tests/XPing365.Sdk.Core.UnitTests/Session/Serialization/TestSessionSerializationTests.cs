using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Session.Serialization;

namespace XPing365.Sdk.Core.UnitTests.Session.Serialization;

internal class TestSessionSerializationTests
{
    [Test]
    public void SerializeThrowsArgumentNullExceptionWhenTestSessionIsNull()
    {
        // Arrange
        TestSession session = null!;
        var serializer = new TestSessionSerializer();

        // Assert
        Assert.Throws<ArgumentNullException>(() => serializer.Serialize(session, Stream.Null, SerializationFormat.XML));
    }

    [Test]
    public void SerializeThrowsArgumentNullExceptionWhenStreamIsNull()
    {
        // Arrange
        TestSession session = new()
        {
            Url = new Uri("https://test"),
            StartDate = DateTime.UtcNow,
            State = TestSessionState.NotStarted,
            Steps = []
        };
        var serializer = new TestSessionSerializer();

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
            serializer.Serialize(
                session,
                stream: null!,
                SerializationFormat.XML));
    }

    [Test]
    public void DeserializeThrowsArgumentNullExceptionWhenStreamIsNull()
    {
        // Arrange
        TestSession session = new()
        {
            Url = new Uri("https://test"),
            StartDate = DateTime.UtcNow,
            State = TestSessionState.NotStarted,
            Steps = []
        };
        var serializer = new TestSessionSerializer();

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
            serializer.Deserialize(
                stream: null!,
                SerializationFormat.XML));
    }

    [Test]
    public void TestSessionIdIsSerialized()
    {
        // Arrange
        TestSession session = new()
        {
            Url = new Uri("https://test"),
            StartDate = DateTime.UtcNow,
            State = TestSessionState.NotStarted,
            Steps = []
        };
        var serializer = new TestSessionSerializer();
        using var stream = new MemoryStream();
        
        // Act
        serializer.Serialize(session, stream, SerializationFormat.XML);
        stream.Position = 0;

        using var reader = new StreamReader(stream, Encoding.UTF8);

        // Load the XML into an XmlDocument
        var doc = new XmlDocument();
        doc.Load(reader);
        
        // Create a NamespaceManager and add the namespace used in the XML
        var namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/XPing365.Sdk.Core.Session");

        // Use the namespace prefix in the XPath expression to select the Id element
        XmlNode? idNode = doc.SelectSingleNode("//ns:TestSession/Id", namespaceManager);

        // Assert
        Assert.That(idNode, Is.Not.Null);
        Assert.That(Guid.TryParse(idNode.InnerText, out _), Is.True);
        Assert.That(Guid.Parse(idNode.InnerText), Is.EqualTo(session.Id));
    }

    [Test]
    public void TestSessionInstanceIsCorrectlySerializedToXml()
    {
        // Arrange
        TestSession session = new()
        {
            Url = new Uri("https://test"),
            StartDate = DateTime.UtcNow,
            State = TestSessionState.NotStarted,
            Steps = [new TestStep
            {
                Name = "step1",
                TestComponentIteration = 1,
                Duration = TimeSpan.Zero,
                PropertyBag = new PropertyBag<IPropertyBagValue>(),
                Result = TestStepResult.Failed,
                StartDate = DateTime.Now,
                Type = TestStepType.ActionStep
            }]
        };

        var serializer = new TestSessionSerializer();
        using var stream = new MemoryStream();

        // Act
        serializer.Serialize(session, stream, SerializationFormat.XML);
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);

        // Load the XML into an XmlDocument
        var doc = new XmlDocument();
        doc.Load(reader);

        // Create a NamespaceManager and add the namespace used in the XML
        var namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/XPing365.Sdk.Core.Session");

        // Use the namespace prefix in the XPath expression to select the Url element
        XmlNode? urlNode = doc.SelectSingleNode("//ns:TestSession/Url", namespaceManager);

        // Assert
        Assert.That(urlNode, Is.Not.Null);
        Assert.That(Uri.TryCreate(urlNode.InnerText, UriKind.Absolute, out _), Is.True);
        Assert.That(new Uri(urlNode.InnerText), Is.EqualTo(session.Url));
    }

    [TestCase(SerializationFormat.XML)]
    [TestCase(SerializationFormat.Binary)]
    [Test]
    public void TestSessionInstanceIsCorrectlyDeserialized(SerializationFormat format)
    {
        // Arrange
        TestSession session = new()
        {
            Url = new Uri("https://test"),
            StartDate = DateTime.UtcNow,
            State = TestSessionState.NotStarted,
            Steps = [new TestStep
            {
                Name = "step1",
                TestComponentIteration = 1,
                Duration = TimeSpan.Zero,
                PropertyBag = new PropertyBag<IPropertyBagValue>(),
                Result = TestStepResult.Failed,
                StartDate = DateTime.Now,
                Type = TestStepType.ActionStep
            }]
        };

        var serializer = new TestSessionSerializer();
        using var stream = new MemoryStream();

        // Act
        serializer.Serialize(session, stream, format);
        stream.Position = 0;
        TestSession? session1 = serializer.Deserialize(stream, format);

        // Assert
        Assert.That(session1, Is.Not.Null);
        Assert.That(session.Url.AbsoluteUri, Is.EqualTo(session1.Url.AbsoluteUri));
        Assert.That(session.StartDate, Is.EqualTo(session1.StartDate));
        Assert.That(session.Steps.Count, Is.EqualTo(session1.Steps.Count));
        Assert.That(session.Steps.First().Name, Is.EqualTo(session1.Steps.First().Name));
        Assert.That(session.Steps.First().Duration, Is.EqualTo(session1.Steps.First().Duration));
        Assert.That(session1.Steps.First().PropertyBag, Is.Not.Null);
        Assert.That(session.Steps.First().StartDate, Is.EqualTo(session1.Steps.First().StartDate));
        Assert.That(session.Steps.First().Type, Is.EqualTo(session1.Steps.First().Type));
    }

    [TestCase(SerializationFormat.XML)]
    [TestCase(SerializationFormat.Binary)]
    [Test]
    public void TestSessionIdIsCorrectlyDeserialized(SerializationFormat format)
    {
        // Arrange
        TestSession session = new()
        {
            Url = new Uri("https://test"),
            StartDate = DateTime.UtcNow,
            State = TestSessionState.NotStarted,
            Steps = []
        };

        var serializer = new TestSessionSerializer();
        using var stream = new MemoryStream();

        // Act
        serializer.Serialize(session, stream, format);
        stream.Position = 0;
        TestSession? session1 = serializer.Deserialize(stream, format);

        // Assert
        Assert.That(session1, Is.Not.Null);
        Assert.That(session.Id, Is.EqualTo(session1.Id));
    }

    [Test]
    public void DeserializerThrowsSerializationExceptionWhenIncorrectSerializationFormatProvided()
    {
        // Arrange
        TestSession session = new()
        {
            Url = new Uri("https://test"),
            StartDate = DateTime.UtcNow,
            State = TestSessionState.NotStarted,
            Steps = [new TestStep
            {
                Name = "step1",
                TestComponentIteration = 1,
                Duration = TimeSpan.Zero,
                PropertyBag = new PropertyBag<IPropertyBagValue>(),
                Result = TestStepResult.Failed,
                StartDate = DateTime.Now,
                Type = TestStepType.ActionStep
            }]
        };

        var serializer = new TestSessionSerializer();
        using var stream = new MemoryStream();

        // Act
        serializer.Serialize(session, stream, SerializationFormat.XML);
        stream.Position = 0;

        Assert.Throws<SerializationException>(() => serializer.Deserialize(stream, SerializationFormat.Binary));
    }
}
