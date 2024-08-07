using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using XPing365.Sdk.Availability.Extensions;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.DependencyInjection;
using XPing365.Sdk.Core.Session;
using XPing365.Sdk.Core.Session.Serialization;

namespace ConsoleAppTesting;

public sealed class Program
{
    const int EXIT_SUCCESS = 0;
    const int EXIT_FAILURE = 1;

    const int MAX_SIZE_IN_BYTES = 153600; // 150kB

    static async Task<int> Main(string[] args)
    {
        IHost host = CreateHostBuilder(args).Build();

        var urlOption = new Option<Uri?>(
            name: "--url",
            description: "A URL address of the page being validated.")
        { 
            IsRequired = true,
        };

        var command = new RootCommand("Sample application for XPing 365");
        command.AddOption(urlOption);
        command.SetHandler(async (InvocationContext context) =>
        {
            Uri url = context.ParseResult.GetValueForOption(urlOption)!;
            var testAgent = host.Services.GetRequiredService<TestAgent>();

            testAgent
                .UseDnsLookup()
                .UseIPAddressAccessibilityCheck()
                .UseHttpClient()
                .UseHttpValidation(response =>
                {
                    response.EnsureSuccessStatusCode();
                    response.Header(HeaderNames.Server).HasValue("Google", new() { Exact = false });
                })
                .UseHtmlValidation(html =>
                {
                    html.HasMaxDocumentSize(MAX_SIZE_IN_BYTES);
                });

            var session = await testAgent.RunAsync(url).ConfigureAwait(false);

            await using (session.ConfigureAwait(false))
            {
                context.Console.WriteLine("\nSummary:");
                context.Console.WriteLine($"{session}");
                context.ExitCode = session.IsValid ? EXIT_SUCCESS : EXIT_FAILURE;
            }
        });

        return await command.InvokeAsync(args).ConfigureAwait(false);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((services) =>
            {
                services.AddSingleton<IProgress<TestStep>, Progress>();
                services.AddHttpClientFactory();
                services.AddTestAgent();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddFilter(typeof(HttpClient).FullName, LogLevel.Warning);
            });
}
