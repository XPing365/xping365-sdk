using SimpleTestsSample.Pages.Components;
using HtmlAgilityPack;
using XPing365.Core.Source;

namespace SimpleTestsSample.Pages
{
    public class HomePage : HtmlSource
    {
        [XPath("//head/title", ReturnType.InnerText)]
        public string? Title { get; set; }
        
        [XPath("//a[@id='nava']/img", "src")]
        public string? LogoUrl { get; set; }
        
        [XPath("//div[@id='navbarExample']/ul")]
        public MainMenu? MainMenu { get; set; }
    }
}
