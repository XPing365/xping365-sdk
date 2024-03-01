using System.Text.RegularExpressions;
using System.Xml.XPath;
using Microsoft.Extensions.DependencyInjection;
using NUnitTestProject.TestSuite;
using XPing365.Sdk.Availability.TestActions;
using XPing365.Sdk.Availability.TestValidators;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;

namespace NUnitTestProject;

[TestFixture]
public partial class IndexPageTests : WebAppIntegrationTestFixture
{
    private TestAgent _agent = null!;

    [SetUp]
    public void Setup()
    {
        _agent = ServiceProvider.GetRequiredService<TestAgent>();
        _agent.Container = new Pipeline(
            name: "WebAppIntegrationTests",
            components: [new HttpRequestSender()]);
    }

    [GeneratedRegex("<h1(.*)>Welcome<\\/h1>")]
    private static partial Regex WelcomeTextIsH1();

    [Test]
    public async Task WelcomeTextIsInH1Tag()
    {
        // Arrange
         _agent.Container?.AddComponent(new RegexContentValidator(
            regex: WelcomeTextIsH1,
            isValid: (matches) => matches.First().Success,
            onError: $"The response content did not match the Regex expression: '{WelcomeTextIsH1()}'"));

        // Act
        TestSession session = await _agent.RunAsync(TestServer, TestSettings.DefaultForBrowser).ConfigureAwait(false);

        // Assert
        Assert.That(session.IsValid, Is.True, session.Failures.FirstOrDefault()?.ErrorMessage);
    }

    [Test]
    public async Task LearnAboutTextIsAvailable()
    {
        // Arrange
        const string learnAboutLabel = "Learn about";

         _agent.Container?.AddComponent(new StringContentValidator(
            isValid: (content) => content.Contains(learnAboutLabel, StringComparison.InvariantCulture),
            onError: $"The response content did not contain the expected '{learnAboutLabel}' text."));

        // Act
        TestSession session = await _agent.RunAsync(TestServer, TestSettings.DefaultForBrowser).ConfigureAwait(false);

        // Assert
        Assert.That(session.IsValid, Is.True, session.Failures.FirstOrDefault()?.ErrorMessage);
    }

    [Test]
    public async Task LoginLinkHasCorrectHrefAttribute()
    {
        // Arrange
        var xpath = XPathExpression.Compile("//ul[@class='navbar-nav']/li/a[@href='/Identity/Account/Login']/text()");

         _agent.Container?.AddComponent(new XPathContentValidator(
            xpath: xpath,
            isValid: (nodes) => nodes.First().InnerText == "Login",
            onError: $"The HTML document does not match the XPath expression: '{xpath.Expression}'"));

        // Act
        TestSession session = await _agent.RunAsync(TestServer, TestSettings.DefaultForBrowser).ConfigureAwait(false);

        // Assert
        Assert.That(session.IsValid, Is.True, session.Failures.FirstOrDefault()?.ErrorMessage);
    }
}