using HtmlAgilityPack;
using XPing365.Core.Parser.Converters;
using XPing365.Core.Parser.Internals;
using XPing365.Core.Source;

namespace XPing365.Core.Tests
{
    public class DefaultParserTests
    {
        class SingleNodePage : HtmlSource
        {
            [XPath("//head/title")]
            public string? Title { get; set; }

            [XPath("//body/h1", "test1")]
            public string? TestAttribute { get; set; }

            [XPath("//body/h1")]
            public string? Header { get; set; }
        }

        class CollectionOfNodesPage : HtmlSource
        {
            [AttributeUsage(AttributeTargets.Property)]
            public class MileageCoverterAttribute : Attribute, IValueConverter
            {
                public object? Convert(string value, Type targetType)
                {
                    if (targetType == typeof(float))
                    {
                        return float.Parse(value.Replace("mpg", ""));
                    }

                    return value;
                }
            }

            public class Car
            {
                [XPath(".//span[@data-type='name']")]
                public string? Name { get; set; }
                [XPath(".//p[@data-type='description']")]
                public string? Description { get; set; }
                [XPath(".//p[@data-type='mileage']")]
                [MileageCoverter]
                public float Mileage { get; set; }
                [XPath(".//span[@data-type='price']")]
                public float Price { get; set; }
            }

            [XPath("//ul/li")]
            public IList<Car>? Cars { get; set; }
        }

        class OptionalItemHasDefaultFromCtorPage : HtmlSource
        {
            public class Car
            {
                public Car()
                {
                    // When data not found then default is value coming from ctor
                    this.Mileage = "n/a";
                }

                [XPath(".//p[@data-type='mileage']")]
                public string Mileage { get; set; }
            }

            [XPath("//ul/li")]
            public IList<Car>? Cars { get; set; }
        }

        class NodeNotFoundPage : HtmlSource
        {
            [XPath("//head/NotFoundTitle")]
            public string? Title { get; set; }
        }

        class NestedClassPage : HtmlSource
        {
            public class Car
            {
                [XPath(".//span[@data-type='name']")]
                public string? Name { get; set; }
            }

            [XPath("//ul/li[3]")]
            public Car? CarItem { get; set; }
        }

        [Test]
        public void SingleNodeTest()
        {
            SingleNodePage page = new() { Html = File.ReadAllText("./Feeds/Parser.html") };

            DefaultParser<SingleNodePage> defaultParser = new();
            Assert.Multiple(() =>
            {
                Assert.That(() => defaultParser.Parse(ref page).Title, Is.EqualTo("Title"));
                Assert.That(() => defaultParser.Parse(ref page).TestAttribute, Is.EqualTo("header"));
                Assert.That(() => defaultParser.Parse(ref page).Header, Is.EqualTo("My First Heading"));
            });
        }

        [Test]
        public void CollectionOfNodesTest()
        {
            CollectionOfNodesPage page = new() { Html = File.ReadAllText("./Feeds/Parser.html") };
            DefaultParser<CollectionOfNodesPage> defaultParser = new();
            Assert.Multiple(() =>
            {
                Assert.That(() => defaultParser.Parse(ref page).Cars?.Count, Is.EqualTo(3));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[0].Name, Is.EqualTo("Ford"));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[0].Description, Is.EqualTo("Description1"));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[0].Price, Is.EqualTo(55.99f));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[2].Mileage, Is.EqualTo(default(float)));
            });
            Assert.Multiple(() =>
            {
                Assert.That(() => defaultParser.Parse(ref page).Cars?.Count, Is.EqualTo(3));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[1].Name, Is.EqualTo("VW"));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[1].Description, Is.EqualTo("Description2"));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[1].Price, Is.EqualTo(75.99f));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[1].Mileage, Is.EqualTo(20f));
            });
            Assert.Multiple(() =>
            {
                Assert.That(() => defaultParser.Parse(ref page).Cars?.Count, Is.EqualTo(3));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[2].Name, Is.EqualTo("Toyota"));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[2].Description, Is.EqualTo("Description3"));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[2].Price, Is.EqualTo(100f));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[2].Mileage, Is.EqualTo(default(float)));
            });
        }

        [Test]
        public void OptionalItemHasDefaultValueFromCtorTest()
        {
            OptionalItemHasDefaultFromCtorPage page = new() { Html = File.ReadAllText("./Feeds/Parser.html") };
            DefaultParser<OptionalItemHasDefaultFromCtorPage> defaultParser = new();
            Assert.Multiple(() =>
            {
                Assert.That(() => defaultParser.Parse(ref page).Cars?[0].Mileage, Is.EqualTo("n/a"));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[1].Mileage, Is.EqualTo("20mpg"));
                Assert.That(() => defaultParser.Parse(ref page).Cars?[2].Mileage, Is.EqualTo("n/a"));
            });
        }

        [Test]
        public void NestedClassPageTest()
        {
            NestedClassPage page = new() { Html = File.ReadAllText("./Feeds/Parser.html") };
            DefaultParser<NestedClassPage> defaultParser = new();

            Assert.That(() => defaultParser.Parse(ref page).CarItem?.Name, Is.EqualTo("Toyota"));
        }

        [Test]
        public void NodeNotFoundTest()
        {
            NodeNotFoundPage page = new() { Html = File.ReadAllText("./Feeds/Parser.html") };
            DefaultParser<NodeNotFoundPage> defaultParser = new();

            Assert.That(() => defaultParser.Parse(ref page).Title, Is.Null);
        }
    }
}
