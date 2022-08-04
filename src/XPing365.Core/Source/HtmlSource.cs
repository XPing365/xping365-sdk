using System.Net;
using HtmlAgilityPack;
using XPing365.Shared;

namespace XPing365.Core.Source
{
    public class HtmlSource : IDataSource
    {
        public string Url { get; set; } = string.Empty;

        public HttpStatusCode ResponseCode { get; set; }

        public bool IsSuccessResponseCode { get; set; }

        public long ResponseSizeInBytes { get; set; }

        public DateTime RequestStartTime { get; set; }

        public DateTime RequestEndTime { get; set; }

        public string? Html { get; set; }

        public HtmlDocument GetHtmlDocument()
        {
            HtmlDocument document = new();
            document.LoadHtml(this.Html.RequireNotNull(nameof(Html)));
            return document;
        }
    }
}
