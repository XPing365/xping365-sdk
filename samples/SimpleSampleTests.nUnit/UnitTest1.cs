namespace SimpleSampleTests.nUnit
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            int dupa = 5;

            Assert.That(dupa, Is.EqualTo(5));

            var testContext = TestContext.CurrentContext;

            Console.WriteLine(testContext.Test.Name);
        }
    }
}