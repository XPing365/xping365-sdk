# Tutorial: Get started with XPing365 SDK

This tutorial demonstrates how to create a .NET console application that utilizes the XPing365.Availability library. You will start by creating a basic test agent and adding a reporting service. Then, you will build upon that foundation by creating a validation pipeline that contains multiple test components which will run test operations to validate server response.

## Prerequisites

- A code editor, such as [Visual Studio Code](https://code.visualstudio.com/) with the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp).
- The [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

Or

- [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/?utm_medium=microsoft&utm_source=learn.microsoft.com&utm_campaign=inline+link&utm_content=download+vs2022) with the _.NET desktop development_ workload installed.

## Create the app

- Create a .NET 8 console app project named "ConsoleApp".

- Create a folder named _ConsoleApp_ for the project, and then open a command prompt in the new folder.

Run the following command:

```console
dotnet new console --framework net8.0
```

## Install the XPing365.Availability package

Run the following command:

```console
dotnet add package XPing365.Availability --prerelease
```

The `--prerelease` option is necessary because the library is still in beta.

Additionally install `CommandLine` package which we will use in our _ConsoleApp_ to parse and handle command line arguments.

```console
dotnet add package System.CommandLine --prerelease
```

For more information on how to use `CommandLine` please follow [Command Line Tutorial](https://learn.microsoft.com/en-us/dotnet/standard/commandline/get-started-tutorial) 

#### Replace the content of the Program.cs with the following code

```csharp
class Program
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

            TestSession session = await testAgent
                .RunAsync(url, settings: TestSettings.DefaultForHttpClient)
                .ConfigureAwait(false);

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
                services.AddHttpClients();
                services.AddTestAgent(
                    name: "TestAgent", builder: (TestAgent agent) =>
                    {
                        agent.Container = new Pipeline(
                            name: "Availability pipeline",
                            components: [
                                new DnsLookup(),
                                new IPAddressAccessibilityCheck(),
                                new HttpClientRequestSender()
                            ]);
                        return agent;
                    });
            })
            .ConfigureLogging(logging =>
            {
                logging.AddFilter(typeof(HttpClient).FullName, LogLevel.Warning);
            });
}
```

#### Add reporting mechanism

Create a new class `Progress.cs` and replace its content with following:

```csharp
class Progress(ILogger<Program> logger) : IProgress<TestStep>
{
    private readonly ILogger<Program> _logger = logger;

    public void Report(TestStep value)
    {
        switch (value.Result)
        {
            case TestStepResult.Succeeded:
                _logger.LogInformation("{Value}", value.ToString());
                break;
            case TestStepResult.Failed:
                _logger.LogError("{Value}", value.ToString());
                break;
        }
    }
}
```

The `IProgress<TestStep>` interface is implemented by this class, which is called on every test step performed by `HttpClientTestAgent` during its testing operation. This allows to monitor the progress of the test execution.

The preceding code we added earlier in `Program.cs` does following:

- Creates a default host builder and adds availability test agent. It also configures logging mechanism to filter logs coming out from `HttpClient`. 

```csharp
static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((services) =>
        {
            services.AddTransient<IProgress<TestStep>, Progress>();
            services.AddHttpClients();
            services.AddTestAgent(
                name: "TestAgent", builder: (TestAgent agent) =>
                {
                    agent.Container = new Pipeline(
                        name: "Availability pipeline",
                        components: [
                            new DnsLookup(),
                            new IPAddressAccessibilityCheck(),
                            new HttpClientRequestSender()
                        ]);
                    return agent;
                });
        })
        .ConfigureLogging(logging =>
        {
            logging.AddFilter(typeof(HttpClient).FullName, LogLevel.Warning);
        });
```

For more information on how to use dependency injection in .NET please follow this [Dependency Injection Tutorial](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage).

- Creates an option named `--url` of type `Uri` and assigns it to the root command:

```csharp
var urlOption = new Option<Uri?>(
    name: "--url",
    description: "A URL address of the page being validated.")
{ IsRequired = true };

var command = new RootCommand("Sample application for XPing365.Availability");
command.AddOption(urlOption);
```

- Specifies the handler method that will be called when the root command is invoked and parses the `url` option:

```csharp
command.SetHandler(async (InvocationContext context) =>
{
    Uri url = context.ParseResult.GetValueForOption(urlOption)!;
    
    (...)
});

return await command.InvokeAsync(args);
```

- Handler method retrieves `HttpClientTestAgent` service and runs availability test operations with default test settings against the `url` value: 

```csharp

command.SetHandler(async (InvocationContext context) =>
{
    (...)

    var testAgent = host.Services.GetRequiredKeyedService<TestAgent>(serviceKey: "TestAgent");

    TestSession session = await testAgent
        .RunAsync(url, settings: TestSettings.DefaultForHttpClient);
});

return await command.InvokeAsync(args);
```

- Prints out summary and specifies exit code depending on the test results:

```csharp
command.SetHandler(async (InvocationContext context) =>
{
    (...)

    context.Console.WriteLine("\nSummary:");
    context.Console.WriteLine($"{session}");
    context.ExitCode = session.IsValid ? EXIT_SUCCESS : EXIT_FAILURE;
});

return await command.InvokeAsync(args);
```

## Test the app

Run the `dotnet build` command, and then open a command prompt in the `ConsoleApp/bin/Debug/net8.0` folder to run the executable:

```console
dotnet build
cd bin/Debug/net8.0
ConsoleApp --url https://demoblaze.com
```

Upon running the application, it performs availability tests on the URL specified by the `--url` option and prints the results:

```console
ConsoleApp.exe --url http://demoblaze.com

info: ConsoleApp.Program[0]
      1/28/2024 4:37:36 PM (45ms) [ActionStep] DNS lookup succeeded.
info: ConsoleApp.Program[0]
      1/28/2024 4:37:36 PM (11ms) [ActionStep] IPAddress accessibility check succeeded.
info: ConsoleApp.Program[0]
      1/28/2024 4:37:36 PM (488ms) [ActionStep] Send HTTP Request succeeded.

Summary:
1/28/2024 4:37:36 PM (545.8241[ms]) Test session completed for http://demoblaze.com/.        
Total steps: 3, Success: 3, Failures: 0
```

## Add validation pipeline

- Add the following code to the `Program` class in the `Program.cs` file to create a `Pipeline` object that consists of the following validation tests:

```csharp
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
```

- `HttpStatusCodeValidator` is used to validate the HTTP status code:

```csharp
new HttpStatusCodeValidator(
    isValid: (HttpStatusCode code) => code == HttpStatusCode.OK,
    errorMessage: (HttpStatusCode code) =>
        $"The HTTP request failed with status code {code}")
```

- `HttpResponseHeadersValidator` is used to validate the response headers:

```csharp
new HttpResponseHeadersValidator(
    isValid: (HttpResponseHeaders headers) => headers.Contains(HeaderNames.Server),
    errorMessage: (HttpResponseHeaders headers) =>
        $"The HTTP response headers did not include the expected $'{HeaderNames.Server}' header.")
```

- `ServerContentResponseValidator` is used to validate the response content:

```csharp
new ServerContentResponseValidator(
    isValid: (byte[] content, HttpContentHeaders contentHeaders) => content.Length < MAX_SIZE_IN_BYTES,
    errorMessage: (byte[] content, HttpContentHeaders contentHeaders) =>
        $"The HTTP response content exceeded the maximum allowed size of {MAX_SIZE_IN_BYTES} bytes.")
```

- In the command handler reference the newly added `Pipeline` in the `HttpClientTestAgent` as follows:

```csharp
testAgent.Container.AddComponent(CreateValidationPipeline());
```

## Test the new app with validation pipeline

Now if you try to run the app, you get additional test steps performed by the `HttpClientTestAgent` in the order in which they were added:

```console
ConsoleApp.exe --url http://demoblaze.com

info: ConsoleApp.Program[0]
      1/28/2024 4:37:36 PM (45ms) [ActionStep] DNS lookup succeeded.
info: ConsoleApp.Program[0]
      1/28/2024 4:37:36 PM (11ms) [ActionStep] IPAddress accessibility check succeeded.
info: ConsoleApp.Program[0]
      1/28/2024 4:37:36 PM (488ms) [ActionStep] Send HTTP Request succeeded.
info: ConsoleApp.Program[0]
      1/28/2024 4:37:37 PM (0ms) [ValidateStep] Http status code validation succeeded.       
info: ConsoleApp.Program[0]
      1/28/2024 4:37:37 PM (1ms) [ValidateStep] Http response headers validation succeeded.  
info: ConsoleApp.Program[0]
      1/28/2024 4:37:37 PM (0ms) [ValidateStep] Server content response validation succeeded.

Summary:
1/28/2024 4:37:36 PM (545.8241[ms]) Test session completed for http://demoblaze.com/.        
Total steps: 6, Success: 6, Failures: 0
```

Congratulations! You have successfully completed this tutorial on how to use the `XPing365.Availability` SDK. You have learned how to create a .NET console application that utilizes the `XPing365.Availability` library to automate web application testing. You have also learned how to create a basic test agent, add a reporting service, and build a validation pipeline that contains multiple test components.

For a complete implementation of this tutorial, please refer to our sample folder in our [repository on GitHub](https://github.com/XPing365/xping365-sdk/tree/main/samples/ConsoleApp).