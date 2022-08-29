using Newtonsoft.Json.Linq;
using XPing365.Core.DataParser.Internal;
using XPing365.Core.DataSource.Internal;

namespace XPing365.Core.Tests
{
    public class XmlTypeTraversalParserTests
    {
        readonly string singleNodeXml =
            @"<Page>
                <Title XPath=""//head/title"" type=""System.String"" />
                <TestAttribute XPath=""//body/h1"" attribute=""test1"" type=""System.String"" />
                <Header XPath=""//body/h1"" type=""System.String"" />
              </Page>";
        readonly string collectionOfNodesPage =
            @"<Page>
                <Cars XPath=""//ul/li"" type=""System.Collections.Generic.List`1"">
                    <Name XPath="".//span[@data-type='name']"" type=""System.String"" />
                    <Description XPath="".//p[@data-type='description']"" type=""System.String"" />
                    <Price XPath="".//span[@data-type='price']"" type=""System.Single"" />
                </Cars>
              </Page>";
        readonly string optionalItemHasDefaultValue =
            @"<Page>
                <Cars XPath=""//ul/li"" type=""System.Collections.Generic.List`1"">
                    <Mileage XPath="".//p[@data-type='mileage']"" type=""System.String"" />
                </Cars>
              </Page>";
        readonly string nestedClassPage =
            @"<Page>
                <Car XPath=""//ul/li[3]"" type=""System.Object"">
                    <Name XPath="".//span[@data-type='name']"" type=""System.String"" />
                </Car>
              </Page>";
        readonly string nodeNotFoundPage =
            @"<Page>
                <Title XPath=""//head/NotFoundTitle"" type=""System.Single"" />
              </Page>";
        readonly string singleNodeForCollection =
            @"<Page>
                <Items XPath=""//head/title"" type=""System.Collections.Generic.List`1"" />
              </Page>";
        readonly string multipleNodesForString =
            @"<Page>
                <Item XPath=""//ul/li"" type=""System.String"" />
              </Page>";
    
        [Test]
        public void SingleNodeTest()
        {
            XPathDefinitionWithXmlConfig page = new()
            {
                Html = File.ReadAllText("./Feeds/Parser.html"),
                XmlConfig = singleNodeXml
            };

            XmlTypeTraversalParser defaultParser = new();
            page = defaultParser.Parse(ref page);

            Assert.That(page.Json, Is.Not.Null);

            dynamic result = JObject.Parse(page.Json);

            Assert.Multiple(() =>
            {
                Assert.That(() => (string)result.Title.Value, Is.EqualTo("Title"));
                Assert.That(() => (string)result.TestAttribute.Value, Is.EqualTo("header"));
                Assert.That(() => (string)result.Header.Value, Is.EqualTo("My First Heading"));
            });
        }

        [Test]
        public void CollectionOfNodesTest()
        {
            XPathDefinitionWithXmlConfig page = new()
            {
                Html = File.ReadAllText("./Feeds/Parser.html"),
                XmlConfig = collectionOfNodesPage
            };

            XmlTypeTraversalParser defaultParser = new();
            page = defaultParser.Parse(ref page);

            Assert.That(page.Json, Is.Not.Null);

            dynamic result = JObject.Parse(page.Json);

            Assert.That(() => (int)result.Cars.Count, Is.EqualTo(3));

            Assert.Multiple(() =>
            {
                Assert.That(() => (string)result.Cars[0].Name, Is.EqualTo("Ford"));
                Assert.That(() => (string)result.Cars[0].Description, Is.EqualTo("Description1"));
                Assert.That(() => (float)result.Cars[0].Price, Is.EqualTo(55.99f));
            });
            Assert.Multiple(() =>
            {
                Assert.That(() => (string)result.Cars[1].Name, Is.EqualTo("VW"));
                Assert.That(() => (string)result.Cars[1].Description, Is.EqualTo("Description2"));
                Assert.That(() => (float)result.Cars[1].Price, Is.EqualTo(75.99f));
            });
            Assert.Multiple(() =>
            {
                Assert.That(() => (string)result.Cars[2].Name, Is.EqualTo("Toyota"));
                Assert.That(() => (string)result.Cars[2].Description, Is.EqualTo("Description3"));
                Assert.That(() => (float)result.Cars[2].Price, Is.EqualTo(100f));
            });
        }

        [Test]
        public void OptionalItemHasDefaultValueFromCtorTest()
        {
            XPathDefinitionWithXmlConfig page = new()
            {
                Html = File.ReadAllText("./Feeds/Parser.html"),
                XmlConfig = optionalItemHasDefaultValue
            };

            XmlTypeTraversalParser defaultParser = new();
            page = defaultParser.Parse(ref page);

            Assert.That(page.Json, Is.Not.Null);

            dynamic result = JObject.Parse(page.Json);
            Assert.Multiple(() =>
            {
                Assert.That(() => (string)result.Cars[0].Mileage, Is.EqualTo(""));
                Assert.That(() => (string)result.Cars[1].Mileage, Is.EqualTo("20mpg"));
                Assert.That(() => (string)result.Cars[2].Mileage, Is.EqualTo(""));
            });
        }

        [Test]
        public void NestedClassPageTest()
        {
            XPathDefinitionWithXmlConfig page = new()
            {
                Html = File.ReadAllText("./Feeds/Parser.html"),
                XmlConfig = nestedClassPage
            };

            XmlTypeTraversalParser defaultParser = new();
            page = defaultParser.Parse(ref page);

            Assert.That(page.Json, Is.Not.Null);

            dynamic result = JObject.Parse(page.Json);

            Assert.That(() => (string)result.Car.Name, Is.EqualTo("Toyota"));
        }

        [Test]
        public void NodeNotFoundTest()
        {
            XPathDefinitionWithXmlConfig page = new()
            {
                Html = File.ReadAllText("./Feeds/Parser.html"),
                XmlConfig = nodeNotFoundPage
            };

            XmlTypeTraversalParser defaultParser = new();
            page = defaultParser.Parse(ref page);

            Assert.That(page.Json, Is.Not.Null);

            dynamic result = JObject.Parse(page.Json);

            Assert.That(() => (string)result.Title, Is.EqualTo(""));
        }

        [Test]
        public void SingleNodeXPathMisplacedForCollectionTypeTest()
        {
            XPathDefinitionWithXmlConfig page = new()
            {
                Html = File.ReadAllText("./Feeds/Parser.html"),
                XmlConfig = singleNodeForCollection
            };

            XmlTypeTraversalParser defaultParser = new();

            Assert.Multiple(() =>
            {
                Assert.That(() => defaultParser.Parse(ref page), Throws.Nothing);
                Assert.That(page.Json, Is.Not.Null);
            });
        }

        [Test]
        public void MultipleNodesXPathMisplacedForStringTest()
        {
            XPathDefinitionWithXmlConfig page = new()
            {
                Html = File.ReadAllText("./Feeds/Parser.html"),
                XmlConfig = multipleNodesForString
            };

            XmlTypeTraversalParser defaultParser = new();

            Assert.Multiple(() =>
            {
                Assert.That(() => defaultParser.Parse(ref page), Throws.Nothing);
                Assert.That(page.Json, Is.Not.Null);
            });
        }
    }
}
