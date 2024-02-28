using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using XPing365.Sdk.Availability.TestActions;
using XPing365.Sdk.Availability.TestValidators;
using XPing365.Sdk.Core;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.DependencyInjection;
using XPing365.Sdk.Core.Session;

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
        { IsRequired = true };

        var command = new RootCommand("Sample application for XPing365.Availability");
        command.AddOption(urlOption);
        command.SetHandler(async (InvocationContext context) =>
        {
            Uri url = context.ParseResult.GetValueForOption(urlOption)!;
            var testAgent = host.Services.GetRequiredKeyedService<TestAgent>(serviceKey: "TestAgent");
            testAgent.Container?.AddComponent(CreateValidationPipeline());

            TestSession session = await testAgent
                .RunAsync(url, settings: TestSettings.DefaultForHttpClient)
                .ConfigureAwait(false);

            context.Console.WriteLine("\nSummary:");
            context.Console.WriteLine($"{session}");
            context.ExitCode = session.IsValid ? EXIT_SUCCESS : EXIT_FAILURE;
        });

        return await command.InvokeAsync(args).ConfigureAwait(false);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((services) =>
            {
                // Register the Progress class as a singleton service
                services.AddSingleton<IProgress<TestStep>, Progress>();
                // Add HttpClients using the IHttpClientFactory
                services.AddHttpClients();
                // Adds a TestAgent service to the service collection and configures its pipeline
                services.AddTestAgent(
                    name: "TestAgent", builder: (TestAgent agent) =>
                    {
                        // Set the container of the TestAgent to a new Pipeline object
                        agent.Container = new Pipeline(
                            name: "Availability pipeline",
                            components: [
                                // Add a DnsLookup component to the pipeline
                                new DnsLookup(),
                                // Add an IPAddressAccessibilityCheck component to the pipeline
                                new IPAddressAccessibilityCheck(),
                                // Add an HttpRequestSender component to the pipeline
                                new HttpRequestSender()
                            ]);
                        // Return the configured TestAgent object
                        return agent;
                    });
            })
            .ConfigureLogging(logging =>
            {
                logging.AddFilter(typeof(HttpClient).FullName, LogLevel.Warning);
            });

    static Pipeline CreateValidationPipeline() =>
        new(name: "Validation pipeline",
            components: [
            new HttpStatusCodeValidator(
                isValid: (HttpStatusCode code) => code == HttpStatusCode.OK,
                onError: (HttpStatusCode code) =>
                    $"The HTTP request failed with status code {code}"),

            new HttpResponseHeadersValidator(
                isValid: (HttpResponseHeaders headers) => headers.Contains(HeaderNames.Server),
                onError: (HttpResponseHeaders headers) =>
                    $"The HTTP response headers did not include the expected $'{HeaderNames.Server}' header."),

            new HttpResponseContentValidator(
                isValid: (byte[] content, HttpContentHeaders contentHeaders) => content.Length < MAX_SIZE_IN_BYTES,
                onError: (byte[] content, HttpContentHeaders contentHeaders) =>
                    $"The HTTP response content exceeded the maximum allowed size of {MAX_SIZE_IN_BYTES} bytes.")]);
}
