using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnitTestProject.TestSuite;
using XPing365.Sdk.Availability;
using XPing365.Sdk.Availability.Extensions;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Session;

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
        TestAgent.TearDown();
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
        var session = await TestAgent.RunAsync(new Uri("http://localhost/privacy"));

        await using (session.ConfigureAwait(false))
        {
            // Assert
            Assert.That(session.IsValid, Is.True, session.Failures.FirstOrDefault()?.ErrorMessage);
        }
    }
}
