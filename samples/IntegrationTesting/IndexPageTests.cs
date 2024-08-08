using Microsoft.AspNetCore.Mvc.Testing;
using NUnitTestProject.TestSuite;
using XPing365.Sdk.Availability;
using XPing365.Sdk.Availability.Extensions;
using XPing365.Sdk.Core;

namespace NUnitTestProject;

[TestFixture]
public partial class IndexPageTests : IntegrationTest<WebApp.Program>
{
    protected override WebApplicationFactory<WebApp.Program> CreateFactory()
    {
        return new WebAppFactory();
    }

    [SetUp]
    public void SetUp()
    {
        TestAgent.UseHttpClient();
    }

    [TearDown]
    public void TearDown()
    {
        TestAgent.Clear();
    }

    [Test]
    public async Task IndexPageHasCorrectTitle()
    {
        // Arrange
        TestAgent.UseHtmlValidation(html =>
        {
            html.HasTitle("Home page - WebApp");
        });

        // Act
        await using var session = await TestAgent.RunAsync(new Uri("http://localhost/"));

        // Assert
        Assert.That(session.IsValid, Is.True, session.Failures.FirstOrDefault()?.ErrorMessage);
    }

    [Test]
    public async Task ReturnsSuccessHttpStatusCode()
    {
        // Arrange
        TestAgent.UseHttpValidation(response =>
        {
            response.EnsureSuccessStatusCode();
        });

        // Act
        await using var session = await TestAgent.RunAsync(new Uri("http://localhost/privacy"));

        // Assert
        Assert.That(session.IsValid, Is.True, session.Failures.FirstOrDefault()?.ErrorMessage);
    }
}
