using System.Runtime.Serialization;
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

        using XmlReader xmlReader = XmlReader.Create(stream);
        var document = new XPathDocument(xmlReader);
        XPathNavigator navigator = document.CreateNavigator();
        XPathNodeIterator nodes = navigator.Select(XPathExpression.Compile("//TestSession/Id"));

        // Assert
        Assert.That(nodes.MoveNext(), Is.True);
        Assert.That(nodes.Current, Is.Not.Null);
        Assert.That(nodes.Current.Value, Is.EqualTo(session.Id.ToString()));
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

        using XmlReader xmlReader = XmlReader.Create(stream);
        var document = new XPathDocument(xmlReader);
        XPathNavigator navigator = document.CreateNavigator();
        XPathNodeIterator nodes = navigator.Select("/TestSession/Url");

        // Assert
        Assert.That(nodes.MoveNext(), Is.True);
        Assert.That(nodes.Current, Is.Not.Null);
        Assert.That(nodes.Current.Value, Is.EqualTo(session.Url.AbsoluteUri));
    }

    [Test]
    public void TestSessionInstanceIsCorrectlyDeserializedFromXml()
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
        TestSession? session1 = serializer.Deserialize(stream, SerializationFormat.XML);

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
