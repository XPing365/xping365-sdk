using Microsoft.Extensions.DependencyInjection;
using SimpleTestsSample.Pages;
using XPing365.Core;

namespace SimpleTestsSample.Tests
{
    [SetUpFixture]
    [TestFixtureSource(typeof(TestFixtureProviders), nameof(TestFixtureProviders.ServiceProvider))]
    internal class HomePageTests
    {
        private readonly IServiceProvider serviceProvider;
        private HomePage? homePage;

        private const int MaxAllowedResponeSizeInBytes = 40000;
        private const int MaxAllowedResponseTimeInSeconds = 5;
        private const int MainMenuExpectedItemsCount = 8;

        private const string ExpectedTitle = "STORE";
        private const string HomeMenuItem = "Home (current)";
        private const string ContactMenuItem = "Contact";
        private const string AboutUsMenuItem = "About us";
        private const string CartMenuItem = "Cart";
        private const string LogInMenuItem = "Log in";
        private const string SignUpMenuItem = "Sign up";

        public HomePageTests(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var webDataRetriever = this.serviceProvider.GetRequiredService<IWebDataRetriever>();
            this.homePage = webDataRetriever.GetFromHtmlAsync<HomePage>("/").Result;
        }

        [Test]
        public void ResponseStatusTest()
        {
            Assert.That(() => this.homePage?.IsSuccessResponseCode, Is.True);
        }

        [Test]
        public void ResponseSizeTest()
        {
            Assert.That(() => this.homePage?.ResponseSizeInBytes, Is.LessThan(MaxAllowedResponeSizeInBytes));
        }

        [Test]
        public void ResponseTimeTest()
        {
            Assert.That(() => this.homePage?.RequestEndTime - this.homePage?.RequestStartTime, 
                Is.LessThan(TimeSpan.FromSeconds(MaxAllowedResponseTimeInSeconds)));
        }

        [Test]
        public void LogoUrlIsNotEmptyTest()
        {
            Assert.That(() => this.homePage?.LogoUrl, Is.Not.Null);
        }

        [Test]
        public void LogoUrlPointsToPNGImageTest()
        {
            Assert.That(() => this.homePage?.LogoUrl, Does.Contain(".png"));
        }

        [Test]
        public void TitleTest()
        {
            Assert.That(() => this.homePage?.Title, Is.EqualTo(ExpectedTitle));
        }

        [Test]
        public void MainMenuHasCorrectNumberOfItemsTest()
        {
            Assert.That(() => this.homePage?.MainMenu?.Items?.Count, Is.EqualTo(MainMenuExpectedItemsCount));
        }

        [Test]
        [TestCase(HomeMenuItem)]
        [TestCase(ContactMenuItem)]
        [TestCase(AboutUsMenuItem)]
        [TestCase(CartMenuItem)]
        [TestCase(LogInMenuItem)]
        [TestCase(SignUpMenuItem)]
        public void MainMenuHaveExpectedItemTest(string menuItem)
        {
            Assert.That(() => this.homePage?.MainMenu?.Items?.Select(i => i.Text), Does.Contain(menuItem));
        }
    }
}
