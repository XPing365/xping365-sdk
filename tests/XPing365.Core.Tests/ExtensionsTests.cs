using HtmlAgilityPack;
using XPing365.Core.Parser.Converters;
using XPing365.Core.Parser.Internals;
using XPing365.Core.Source;

namespace XPing365.Core.Tests
{
    public class ExtensionsTests
    {
        class NoDefaultCtor
        {
            public readonly string p;
            public NoDefaultCtor(string p) { this.p = p; }
        }

        class DefaultCtor
        {
            public DefaultCtor() { }
        }

        class PropertiesToVisit : HtmlSource // 1 property to visit
        {
            [XPath("")]
            public string? XPathProperty { get; set; }
        }

        class PropertiesToVisitStringWithNoXPath // 0 properites to visit (no XPath attribute)
        {
            public string? Property { get; set; }
        }

        class PropertiesToVisitIntWithNoXPath // 0 properites to visit (no XPath attribute)
        {
            public int Property { get; set; }
        }

        class PropertiesToVisitClassWithDefaultCtor // 1 property to visit, class `DefaultCtor` might have other properties decorated with XPath
        {
            public DefaultCtor? Property { get; set; }
        }

        class PropertiesToVisitClassWithDefaultCtorNoSetter // 0 properites to visit (no setter)
        {
            public DefaultCtor? Property { get; }
        }

        class PropertiesToVisitClassWithDefaultCtorWithXPath // 1 property to visit
        {
            [XPath("")]
            public DefaultCtor? Property { get; set; }
        }

        class PropertiesToVisitClassWithNoDefaultCtor // 0 properites to visit (no default ctor)
        {
            public NoDefaultCtor? Property { get; set; }
        }

        class PropertiesToVisitClassWithNoDefaultCtorWithXPath // should throw an exception because the property is decorated with XPath
                                                               // but it is missing default ctor
        {
            [XPath("")]
            public NoDefaultCtor? Property { get; set; }
        }

        class PropertiesToVisitValueTypeWithXPath : HtmlSource // 1 property to visit
        {
            [XPath("")]
            public int Property { get; set; }
        }

        class PropertiesToVisitValueTypeWithXPathNoSetter : HtmlSource // should throw an exception because the property is decorated with XPath
                                                                           // but it is missing setter
        {
            [XPath("")]
            public int Property { get; }
        }

        class PropertiesToVisitStringTypeWithXPathNoSetter : HtmlSource // should throw an exception because the property is decorated with XPath
                                                                            // but it is missing setter
        {
            [XPath("")]
            public string? Property { get; }
        }

        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        class ValueConverterAttribute : Attribute, IValueConverter
        {
            public object? Convert(string value, Type targetType)
            {
                return value;
            }
        }

        class ClassConverterTest
        {
            [XPath("")]
            [ValueConverter]
            public string? PropertyA { get; set; }

            [XPath("")]
            public string? PropertyB { get; set; }
        }

        [Test]
        public void IsListFromIListTest()
        {
            Assert.That(() => typeof(IList<string>).IsList(), Is.True);
        }

        [Test]
        public void IsListFromListTest()
        {
            Assert.That(() => typeof(List<string>).IsList(), Is.True);
        }

        [Test]
        public void IsListFromIEnumerableTest()
        {
            Assert.That(() => typeof(IEnumerable<string>).IsList(), Is.False);
        }

        [Test]
        public void IsListFromNonGenericListTest()
        {
            Assert.That(() => typeof(System.Collections.IList).IsList(), Is.False);
        }

        [Test]
        public void CreateListFromStringTypeTest()
        {
            Assert.That(() => typeof(string).CreateList(), Is.Null);
        }

        [Test]
        public void CreateListFromGenericListTypeTest()
        {
            Assert.That(() => typeof(IList<string>).CreateList(), Is.Not.Null);
        }

        [Test]
        public void HasDefaultCtorFromIntType()
        {
            Assert.That(() => typeof(int).HasDefaultConstructor(), Is.True);
        }

        [Test]
        public void HasDefaultCtorFromStringType()
        {
            Assert.That(() => typeof(string).HasDefaultConstructor(), Is.False);
        }

        [Test]
        public void HasDefaultCtorFromClassWithoutDefaultCtorType()
        {
            Assert.That(() => typeof(NoDefaultCtor).HasDefaultConstructor(), Is.False);
        }

        [Test]
        public void HasDefaultCtorFromClassWithDefaultCtorType()
        {
            Assert.That(() => typeof(DefaultCtor).HasDefaultConstructor(), Is.True);
        }

        [Test]
        public void CreateListItemFromStringTypeTest()
        {
            Assert.That(() => typeof(string).CreateListItem(), Is.Null);
        }

        [Test]
        public void CreateListItemFromGenericListOfNonReferenceTypesTest()
        {
            Assert.That(() => typeof(IList<int>).CreateListItem(), Is.EqualTo(default(int)));
        }

        [Test]
        public void CreateListItemFromGenericListOfStringTypesTest()
        {
            Assert.That(() => typeof(IList<string>).CreateListItem(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void CreateListItemFromGenericListOfClassNoDefaultCtorTypeTest()
        {
            Assert.That(() => typeof(IList<NoDefaultCtor>).CreateListItem(), Throws.InvalidOperationException);
        }

        [Test]
        public void CreateListItemFromGenericListOfClassWithDefaultCtorTypeTest()
        {
            Assert.That(() => typeof(IList<DefaultCtor>).CreateListItem(), Is.InstanceOf<DefaultCtor>());
        }

        [Test]
        public void GetPropertiesToVisitTest()
        {
            Assert.That(() => typeof(PropertiesToVisit).GetPropertiesToVisit().Count, Is.EqualTo(1));
        }

        [Test]
        public void GetPropertiesToVisitStringWithNoXPathTest()
        {
            Assert.That(() => typeof(PropertiesToVisitStringWithNoXPath).GetPropertiesToVisit().Count, Is.EqualTo(0));
        }

        [Test]
        public void GetPropertiesToVisitIntWithNoXPathTest()
        {
            Assert.That(() => typeof(PropertiesToVisitIntWithNoXPath).GetPropertiesToVisit().Count, Is.EqualTo(0));
        }

        [Test]
        public void GetPropertiesToVisitClassWithDefaultCtorNoXpathTest()
        {
            Assert.That(() => typeof(PropertiesToVisitClassWithDefaultCtor).GetPropertiesToVisit().Count, Is.EqualTo(1));
        }

        [Test]
        public void GetPropertiesToVisitClassWithDefaultCtorNoSetterTest()
        {
            Assert.That(() => typeof(PropertiesToVisitClassWithDefaultCtorNoSetter).GetPropertiesToVisit().Count, Is.EqualTo(0));
        }

        [Test]
        public void GetPropertiesToVisitClassWithDefaultCtorWithXPathTest()
        {
            Assert.That(() => typeof(PropertiesToVisitClassWithDefaultCtorWithXPath).GetPropertiesToVisit().Count, Is.EqualTo(1));
        }

        [Test]
        public void GetPropertiesToVisitClassWithNoDefaultCtorTest()
        {
            Assert.That(() => typeof(PropertiesToVisitClassWithNoDefaultCtor).GetPropertiesToVisit().Count, Is.EqualTo(0));
        }

        [Test]
        public void GetPropertiesToVisitClassWithNoDefaultCtorWithXPathTest()
        {
            Assert.That(() => typeof(PropertiesToVisitClassWithNoDefaultCtorWithXPath).GetPropertiesToVisit().Count, Throws.ArgumentException);
        }

        [Test]
        public void GetPropertiesToVisitWithValueTypeWithXPathTest()
        {
            Assert.That(() => typeof(PropertiesToVisitValueTypeWithXPath).GetPropertiesToVisit().Count, Is.EqualTo(1));
        }

        [Test]
        public void GetPropertiesToVisitWithValueTypeWithXPathNoSetterTest()
        {
            Assert.That(() => typeof(PropertiesToVisitValueTypeWithXPathNoSetter).GetPropertiesToVisit().Count, Throws.ArgumentException);
        }

        [Test]
        public void GetPropertiesToVisitWithStringTypeWithXPathNoSetterTest()
        {
            Assert.That(() => typeof(PropertiesToVisitStringTypeWithXPathNoSetter).GetPropertiesToVisit().Count, Throws.ArgumentException);
        }

        [Test]
        public void GetAttributeTest()
        {
            Assert.That(() => typeof(PropertiesToVisit).GetPropertiesToVisit().First().GetAttribute<XPathAttribute>(), Is.Not.Null);
        }

        [Test]
        public void GetAttributeWithNoXpathTest()
        {
            Assert.That(() => typeof(PropertiesToVisitClassWithDefaultCtor).GetPropertiesToVisit().First().GetAttribute<XPathAttribute>(), Is.Null);
        }

        [Test]
        public void GetConverterExistTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => typeof(ClassConverterTest).GetPropertiesToVisit()[0].GetConverter(), Is.Not.Null);
                Assert.That(() => typeof(ClassConverterTest).GetPropertiesToVisit()[0].GetConverter(), Is.TypeOf<ValueConverterAttribute>());
            });
        }

        [Test]
        public void GetConverterNotExistTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(() => typeof(ClassConverterTest).GetPropertiesToVisit()[1].GetConverter(), Is.Not.Null);
                Assert.That(() => typeof(ClassConverterTest).GetPropertiesToVisit()[1].GetConverter(), Is.TypeOf<DefaultValueConverter>());
            });
        }
    }
}
