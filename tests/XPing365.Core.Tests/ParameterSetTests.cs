using XPing365.Core.Parameter;
using XPing365.Core.Parameter.Internal;

namespace XPing365.Core.Tests
{
    public class ParameterSetTests
    {
        [Test]
        public void NameIsNullTest()
        {
            Assert.That(() => new ParameterSet(name: null!, rawValues: new List<string>()), Throws.ArgumentNullException);
        }

        [Test]
        public void RawValuesIsNullTest()
        {
            Assert.That(() => new ParameterSet(name: "name", rawValues: null!), Throws.ArgumentNullException);
        }

        [Test]
        public void BothValuesAreNullTest()
        {
            Assert.That(() => new ParameterSet(name: null!, rawValues: null!), Throws.ArgumentNullException);
        }

        [Test]
        public void NameIsEmptyTest()
        {
            Assert.That(() => new ParameterSet(name: string.Empty, rawValues: new List<string>()), Throws.ArgumentException);
        }

        [Test]
        public void NameIsWhitespaceTest()
        {
            Assert.That(() => new ParameterSet(name: "     ", rawValues: new List<string>()), Throws.ArgumentException);
        }

        [Test]
        public void RawValuesIsEmptyTest()
        {
            Assert.That(() => new ParameterSet(name: "name", rawValues: Array.Empty<string>()), Throws.ArgumentException);
        }

        [Test]
        public void ParameterSetSuccessTest()
        {
            Assert.That(() => new ParameterSet(name: "name", rawValues: new string[] { "param_value" }), Is.Not.Null);
        }

        [Test]
        public void UrlIsNullWhileCreatingParameterBuilderTest()
        {
            IParameterSet parameter = new ParameterSet(name: "name", rawValues: new string[] { "param_value" });

            Assert.That(() => parameter.CreateBuilder(url: null!), Throws.ArgumentNullException);
        }

        [Test]
        public void CreatingParameterBuilderSuccessTest()
        {
            IParameterSet parameter = new ParameterSet(name: "name", rawValues: new string[] { "param_value" });

            Assert.That(() => parameter.CreateBuilder(url: "example.com"), Is.TypeOf<DefaultParameterSetBuilder>());
        }
    }
}
