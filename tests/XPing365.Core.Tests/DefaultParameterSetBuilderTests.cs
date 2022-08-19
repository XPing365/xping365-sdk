using XPing365.Core.Parameter;
using XPing365.Core.Parameter.Internal;

namespace XPing365.Core.Tests
{
    public class DefaultParameterSetBuilderTests
    {
        [Test]
        public void UrlIsNullTest()
        {
            Assert.That(() => new DefaultParameterSetBuilder(url: null!, parameterSet: Mock.Of<IParameterSet>()), Throws.ArgumentNullException);
        }

        [Test]
        public void ParameterSetIsNullTest()
        {
            Assert.That(() => new DefaultParameterSetBuilder(url: "example.com", parameterSet: null!), Throws.ArgumentNullException);
        }

        [Test]
        public void ParameterNameNotFoundInTheUrlQueryTest()
        {
            ParameterSet parameterSet = new (name: "notFound", rawValues: new string[] { "a", "b", "c" });

            Assert.That(() => new DefaultParameterSetBuilder(url: "example.com/q={q}", parameterSet).Build().Count, Is.EqualTo(0));
        }

        [Test]
        public void ParameterNameIsSameAsInTheUrlQueryTest()
        {
            ParameterSet parameterSet = new (name: "q", rawValues: new string[] { "a", "b", "c" });

            Assert.That(() => new DefaultParameterSetBuilder(url: "example.com/q={q}", parameterSet).Build().Count, Is.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(() => new DefaultParameterSetBuilder(url: "example.com/q={q}", parameterSet).Build()[0], Is.EqualTo("example.com/q=a"));
                Assert.That(() => new DefaultParameterSetBuilder(url: "example.com/q={q}", parameterSet).Build()[1], Is.EqualTo("example.com/q=b"));
                Assert.That(() => new DefaultParameterSetBuilder(url: "example.com/q={q}", parameterSet).Build()[2], Is.EqualTo("example.com/q=c"));
            });
        }
    }
}
