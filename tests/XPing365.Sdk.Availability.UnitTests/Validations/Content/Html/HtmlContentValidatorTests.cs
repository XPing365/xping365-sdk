using System.Text;
using Moq;
using XPing365.Sdk.Availability.UnitTests.Helpers;
using XPing365.Sdk.Availability.Validations.Content.Html;
using XPing365.Sdk.Core.Common;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;
using TestContext = XPing365.Sdk.Core.Components.TestContext;

namespace XPing365.Sdk.Availability.UnitTests.Validations.Content.Html;

public sealed class HtmlContentValidatorTests
{
    private Uri _url;
    private TestSettings _settings;
    private TestContext _context;
    private IServiceProvider _serviceProvider;

    private sealed class HtmlContentValidatorUnderTest(Action<IHtmlContent> validation) : 
        HtmlContentValidator(validation)
    {
        public Action<string, TestContext, string>? OnCreateHtmlContent { get; set; }

        protected override IHtmlContent CreateHtmlContent(string content, TestContext context, string testIdAttribute)
        {
            OnCreateHtmlContent?.Invoke(content, context, testIdAttribute);
            return base.CreateHtmlContent(content, context, testIdAttribute);
        }
    }

    [SetUp]
    public void SetUp()
    {
        _url = new("http://localhost");
        _settings = new();
        _context = new(Mock.Of<ITestSessionBuilder>(), Mock.Of<IInstrumentation>());
        _serviceProvider = Mock.Of<IServiceProvider>();
    }

    [Test]
    public void CtorThrowsArgumentNullExceptionWhenValidationFunctionIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new HtmlContentValidator(validation: null!));
    }

    [Test]
    public void HandleAsyncThrowsArgumentNullExceptionWhenUrlIsNull()
    {
        // Arrange
        var validator = new HtmlContentValidator((IHtmlContent content) => { });

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            validator.HandleAsync(url: null!, _settings, _context, _serviceProvider);
        });
    }

    [Test]
    public void HandleAsyncThrowsArgumentNullExceptionWhenSettingsIsNull()
    {
        // Arrange
        var validator = new HtmlContentValidator((IHtmlContent content) => { });

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            validator.HandleAsync(_url, settings: null!, _context, _serviceProvider);
        });
    }

    [Test]
    public void HandleAsyncThrowsArgumentNullExceptionWhenContextIsNull()
    {
        // Arrange
        var validator = new HtmlContentValidator((IHtmlContent content) => { });

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            validator.HandleAsync(_url, _settings, context: null!, _serviceProvider);
        });
    }

    [Test]
    public void HandleAsyncThrowsArgumentNullExceptionWhenServiceProviderIsNull()
    {
        // Arrange
        var validator = new HtmlContentValidator((IHtmlContent content) => { });

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            validator.HandleAsync(_url, _settings, _context, serviceProvider: null!);
        });
    }

    [Test]
    public async Task HandleAsyncInsufficientDataWhenHttpResponseAndHttpContentAreMissing()
    {
        // Arrange
        var validator = new HtmlContentValidator((IHtmlContent content) => { });
        var mockSessionBuilder =
            Mock.Get(_context.SessionBuilder)
                .Setup(_m => _m.Steps)
                .Returns([]); // Returns an empty steps collection

        // Act
        await validator.HandleAsync(_url, _settings, _context, _serviceProvider);

        // Assert
        Mock.Get(_context.SessionBuilder)
            .Verify(_m => _m.Build(It.Is<Error>(_p => _p == Errors.InsufficientData(validator))));
    }

    [Test]
    public async Task HandleAsyncInsufficientDataWhenHttpResponseIsMissingAndHttpContentIsPresent()
    {
        // Arrange
        var testStep = HtmlContentTestsHelpers.CreateTestStep(
            PropertyBagKeys.HttpContent,
            new PropertyBagValue<byte[]>(Encoding.UTF8.GetBytes("Data")));

        var validator = new HtmlContentValidator((IHtmlContent content) => { });
        var mockSessionBuilder =
            Mock.Get(_context.SessionBuilder)
                .Setup(_m => _m.Steps)
                .Returns([testStep]);

        // Act
        await validator.HandleAsync(_url, _settings, _context, _serviceProvider);

        // Assert
        Mock.Get(_context.SessionBuilder)
            .Verify(_m => _m.Build(It.Is<Error>(_p => _p == Errors.InsufficientData(validator))));
    }

    [Test]
    public async Task HandleAsyncInsufficientDataWhenHttpResponseIsPresentAndHttpContentIsMissing()
    {
        // Arrange
        using var httpResponseMessage = new HttpResponseMessage();

        var testStep = HtmlContentTestsHelpers.CreateTestStep(
            PropertyBagKeys.HttpResponseMessage,
            new NonSerializable<HttpResponseMessage>(httpResponseMessage));

        var validator = new HtmlContentValidator((IHtmlContent content) => { });
        var mockSessionBuilder =
            Mock.Get(_context.SessionBuilder)
                .Setup(_m => _m.Steps)
                .Returns([testStep]);

        // Act
        await validator.HandleAsync(_url, _settings, _context, _serviceProvider);

        // Assert
        Mock.Get(_context.SessionBuilder)
            .Verify(_m => _m.Build(It.Is<Error>(_p => _p == Errors.InsufficientData(validator))));
    }

    [Test]
    public async Task HandleAsyncCallsValidationWhenHttpResponseMessageAndHttpContentArePresent()
    {
        // Arrange
        using var httpResponseMessage = new HttpResponseMessage();

        var testStep = HtmlContentTestsHelpers.CreateTestStep(
            new Dictionary<PropertyBagKey, IPropertyBagValue>()
            {
                { PropertyBagKeys.HttpResponseMessage, new NonSerializable<HttpResponseMessage>(httpResponseMessage) },
                { PropertyBagKeys.HttpContent, new PropertyBagValue<byte[]>(Encoding.UTF8.GetBytes("Data")) }
            });

        bool validationCalled = false;
        var validator = new HtmlContentValidator((IHtmlContent content) => { validationCalled = true; });
        var mockSessionBuilder =
            Mock.Get(_context.SessionBuilder)
                .Setup(_m => _m.Steps)
                .Returns([testStep]);

        // Act
        await validator.HandleAsync(_url, _settings, _context, _serviceProvider);

        // Assert
        Assert.That(validationCalled, Is.True);
    }

    [Test]
    public async Task HandleAsyncCallsSessionBuilderBuildWhenNoStepsHaveBeenCreatedDuringValidation()
    {
        // Arrange
        using var httpResponseMessage = new HttpResponseMessage();

        var testStep = HtmlContentTestsHelpers.CreateTestStep(
            new Dictionary<PropertyBagKey, IPropertyBagValue>()
            {
                { PropertyBagKeys.HttpResponseMessage, new NonSerializable<HttpResponseMessage>(httpResponseMessage) },
                { PropertyBagKeys.HttpContent, new PropertyBagValue<byte[]>(Encoding.UTF8.GetBytes("Data")) }
            });

        var validator = new HtmlContentValidator((IHtmlContent content) =>
        {
            // When validation happens reset number of steps to simulate no steps have been created during validation.
            Mock.Get(_context.SessionBuilder)
                .Setup(_m => _m.Steps)
                .Returns([]); // Simulate no steps after validation
        });

        Mock.Get(_context.SessionBuilder)
            .Setup(_m => _m.Steps)
            .Returns([testStep]);

        // Act
        await validator.HandleAsync(_url, _settings, _context, _serviceProvider);

        // Assert
        Mock.Get(_context.SessionBuilder)
            .Verify(_m => _m.Build(), Times.Once);
    }

    [Test]
    public async Task HandleAsyncPassesHttpContentFromPropertyBag()
    {
        // Arrange
        string expectedData = "Data";
        using var httpResponseMessage = new HttpResponseMessage();

        var testStep = HtmlContentTestsHelpers.CreateTestStep(
            new Dictionary<PropertyBagKey, IPropertyBagValue>()
            {
                { PropertyBagKeys.HttpResponseMessage, new NonSerializable<HttpResponseMessage>(httpResponseMessage) },
                { PropertyBagKeys.HttpContent, new PropertyBagValue<byte[]>(Encoding.UTF8.GetBytes(expectedData)) }
            });
        string receivedContent = string.Empty;
        var validatorUnderTest = new HtmlContentValidatorUnderTest((IHtmlContent content) => { })
        {
            OnCreateHtmlContent = (string content, TestContext context, string testAttributeId) =>
            {
                receivedContent = content;
            }
        };

        Mock.Get(_context.SessionBuilder)
            .Setup(_m => _m.Steps)
            .Returns([testStep]);

        // Act
        await validatorUnderTest.HandleAsync(_url, _settings, _context, _serviceProvider);

        // Assert
        Assert.That(receivedContent, Is.EqualTo(expectedData));
    }
}
