using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XPing365.Core;
using XPing365.Core.Parser;

namespace SimpleTestsSample
{
    public static class TestFixtureProviders
    {
        private static readonly Lazy<IHost> HostInstance = new(valueFactory: () => Initialize(), isThreadSafe: true);

        public static IServiceProvider[] ServiceProvider 
        {            
            get { return new[] { HostInstance.Value.Services }; }
        }

        private static IHost Initialize()
        {
            var host = Host.CreateDefaultBuilder()
                           .ConfigureServices(services =>
                           {
                               services.AddHttpClient<IWebDataRetriever, WebDataRetriever>("httpClient", client =>
                               {
                                   client.BaseAddress = new Uri("https://www.demoblaze.com/");
                               });
                               services.AddTransient<IParserFactory, ParserFactory>();
                               services.AddTransient<IWebDataRetriever, WebDataRetriever>();
                           })
                           .Build();
            return host;
        }
    }
}
