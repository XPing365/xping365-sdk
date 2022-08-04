using HtmlAgilityPack;

namespace SimpleTestsSample.Pages.Components
{
    public class MainMenu
    {
        public class MenuItem
        {
            [XPath("./a", ReturnType.InnerText)]
            public string? Text { get; set; }
            [XPath("./a", "href")]
            public string? Link { get; set; }
        }

        [XPath(".//li")]
        public IList<MenuItem>? Items { get; set; }
    }
}
