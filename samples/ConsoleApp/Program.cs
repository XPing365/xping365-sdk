using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using XPing365.Sdk.Availability;
using XPing365.Sdk.Availability.DependencyInjection;
using XPing365.Sdk.Availability.Validators;
using XPing365.Sdk.Core.Components;
using XPing365.Sdk.Core.Components.Session;

namespace ConsoleApp;

sealed class Program
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
            var testAgent = host.Services.GetRequiredService<AvailabilityTestAgent>();
            testAgent.Container.AddComponent(CreateValidationPipeline());

            TestSession session = await testAgent
                .RunAsync(url, settings: TestSettings.DefaultForAvailability);
            context.Console.WriteLine("\nSummary:");
            context.Console.WriteLine($"{session}");
            context.ExitCode = session.IsValid ? EXIT_SUCCESS : EXIT_FAILURE;
        });

        return await command.InvokeAsync(args);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((services) =>
            {
                services.AddTransient<IProgress<TestStep>, Progress>();
                services.AddAvailabilityTestAgent();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddFilter(typeof(HttpClient).FullName, LogLevel.Warning);
            });

    static Pipeline CreateValidationPipeline() =>
        new(components: [
            new HttpStatusCodeValidator(
                isValid: (HttpStatusCode code) => code == HttpStatusCode.OK,
                errorMessage: (HttpStatusCode code) =>
                    $"The HTTP request failed with status code {code}"),

            new HttpResponseHeadersValidator(
                isValid: (HttpResponseHeaders headers) => headers.Contains(HeaderNames.Server),
                errorMessage: (HttpResponseHeaders headers) =>
                    $"The HTTP response headers did not include the expected $'{HeaderNames.Server}' header."),

            new ServerContentResponseValidator(
                isValid: (byte[] content, HttpContentHeaders contentHeaders) => content.Length < MAX_SIZE_IN_BYTES,
                errorMessage: (byte[] content, HttpContentHeaders contentHeaders) =>
                    $"The HTTP response content exceeded the maximum allowed size of {MAX_SIZE_IN_BYTES} bytes.")]);
}
