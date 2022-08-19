using XPing365.Shared;

namespace XPing365.Core.Tests
{
    public class RetryTests
    {
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void RetryActionWithSpecifiedNumberOfTriesTest(int retryCount)
        {
            int counter = 0;

            Task<int> Run()
            {
                counter++;
                throw new InvalidOperationException();
            };

            Assert.Multiple(() =>
            {
                Assert.That(() => Retry.DoAsync<int>(Run, TimeSpan.Zero, retryCount), Throws.Exception);
                Assert.That(counter, Is.EqualTo(retryCount));
            });
        }

        [Test]
        public void RetryActionWithZeroNumberOfTriesTest()
        {
            int retryCount = 0;
            int counter = 0;

            Task<int> Run()
            {
                counter++;
                throw new InvalidOperationException();
            };

            Assert.Multiple(() =>
            {
                Assert.That(() => Retry.DoAsync<int>(Run, TimeSpan.Zero, retryCount), Throws.ArgumentException);
                Assert.That(counter, Is.EqualTo(retryCount));
            });
        }

        [Test]
        public void RetryActionWithSpecifiedAmountOfIntervalTest()
        {
            TimeSpan interval = TimeSpan.FromSeconds(5);
            int retryCount = 2;
            int counter = 0;
            DateTime start = DateTime.Now;
            TimeSpan delay = TimeSpan.Zero;

            Task<int> Run()
            {
                if (counter++ != 0)
                {
                    delay = DateTime.Now - start;
                }
                throw new InvalidOperationException();
            };

            Assert.Multiple(() =>
            {
                Assert.That(() => Retry.DoAsync<int>(Run, interval, retryCount), Throws.Exception);
                Assert.That(delay, Is.GreaterThanOrEqualTo(interval));
            });
        }

        [Test]
        public void RetrySuccessActionTest()
        {
            const int expectedResult = 1;
            int result = default;

            static Task<int> Run()
            {
                return Task.FromResult(expectedResult);
            };

            Assert.Multiple(() =>
            {
                Assert.That(async () => result = await Retry.DoAsync(Run, TimeSpan.Zero), Throws.Nothing);
                Assert.That(result, Is.EqualTo(expectedResult));
            });
        }
    }
}
