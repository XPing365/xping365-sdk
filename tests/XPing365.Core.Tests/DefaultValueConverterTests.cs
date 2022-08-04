using XPing365.Core.Parser.Converters;

namespace XPing365.Core.Tests
{
    public class DefaultValueConverterTests
    {
        private readonly DefaultValueConverter converter = new();
        
        [Test]
        public void ValueIsNulltest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => this.converter.Convert(value: null!, typeof(string)) == null);
                Assert.That(() => this.converter.Convert(value: null!, typeof(string)), Throws.Nothing);
            });
        }

        [Test]
        public void ValueIsNotNullStringTest()
        {
            Assert.That(() => (string?)this.converter.Convert(value: "test", typeof(string)) == "test");
        }

        [Test]
        public void ValueIsNotNullIntegerTest()
        {
            Assert.That(() => (int?)this.converter.Convert(value: "5", typeof(int)) == 5);
        }

        [Test]
        public void ValueIsNotNullFloatTest()
        {
            Assert.That(() => (float?)this.converter.Convert(value: "5.5", typeof(float)) == 5.5f);
        }

        [Test]
        public void ValueIsNotNullDoubleTest()
        {
            Assert.That(() => (double?)this.converter.Convert(value: "5.5", typeof(double)) == 5.5d);
        }

        [Test]
        public void ValueIsNotNullBooleanStringEqualTrueTest()
        {
            Assert.That(() => (bool?)this.converter.Convert(value: "true", typeof(bool)) == true);
        }

        [Test]
        public void ValueIsNotNullBooleanStringEqualFalseTest()
        {
            Assert.That(() => (bool?)this.converter.Convert(value: "false", typeof(bool)) == false);
        }

        [Test]
        public void ValueIsNotNullDifferentThenZeroBooleanTest()
        {
            Assert.That(() => (bool?)this.converter.Convert(value: "5", typeof(bool)) == true);
        }

        [Test]
        public void ValueIsNotNullEqualZeroBooleanTest()
        {
            Assert.That(() => (bool?)this.converter.Convert(value: "0", typeof(bool)) == false);
        }

        [Test]
        public void ValueTypeIsIncorrectTest()
        {
            Assert.That(() => (bool?)this.converter.Convert(value: "test", typeof(bool)), Throws.Exception.TypeOf<InvalidCastException>());
        }
    }
}