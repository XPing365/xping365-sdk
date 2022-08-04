using HtmlAgilityPack;
using Moq;
using XPing365.Core.Parser.Internals;

namespace XPing365.Core.Tests
{
    public class HtmlNodeExtensionsTests
    {
        private readonly HtmlDocument document;

        public HtmlNodeExtensionsTests()
        {
            this.document = new HtmlDocument();
            this.document.Load("./Feeds/Basic.html");
        }

        [Test]
        public void GetNodeAttributeValueWhenNodeIsNullTest()
        {
            HtmlNode node = null!;

            Assert.Multiple(() =>
            {
                Assert.That(() => node.GetNodeAttributeValue(new XPathAttribute("")), Throws.Nothing);
                Assert.That(() => node.GetNodeAttributeValue(new XPathAttribute("")), Is.Empty);
            });
        }

        [Test]
        public void GetNodeAttributeValueWhenAttributeIsNullTest()
        {
            HtmlNode node = this.document.DocumentNode;

            Assert.That(() => node.GetNodeAttributeValue(null!), Throws.ArgumentNullException);
        }

        [Test]
        public void GetNodeAttributeValueWhenAttributeNotFoundTest()
        {
            XPathAttribute attribute = new XPathAttribute("//body/h1", "not-found");
            HtmlNode node = this.document.DocumentNode.SelectSingleNode(attribute.XPath);

            Assert.That(() => node.GetNodeAttributeValue(attribute), Is.Empty);
        }

        [Test]
        public void GetNodeAttributeValueIsTrimmedTest()
        {
            XPathAttribute attribute = new XPathAttribute("//body/h2", "test2");
            HtmlNode node = this.document.DocumentNode.SelectSingleNode(attribute.XPath);

            Assert.That(() => node.GetNodeAttributeValue(attribute), Is.EqualTo("trimming test"));
        }

        [Test]
        public void GetNodeAttributeValueWhenValueNotProvidedTest()
        {
            XPathAttribute attribute = new XPathAttribute("//body/h3", "test3");
            HtmlNode node = this.document.DocumentNode.SelectSingleNode(attribute.XPath);

            Assert.That(() => node.GetNodeAttributeValue(attribute), Is.Empty);
        }

        [Test]
        public void GetNodeAttributeValueSuccessTest()
        {
            HtmlNode node = this.document.DocumentNode.SelectSingleNode("//body/h1");

            Assert.That(() => node.GetNodeAttributeValue(new XPathAttribute("//body/h1", "test1")), Is.EqualTo("header"));
        }
    }
}
