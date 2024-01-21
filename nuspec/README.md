## About The Project

<b>XPing365 SDK</b> provides a set of tools to make it easy to write automated tests for Web Application and Web API, as well as troubleshoot issues that may arise during testing. The library provides a number of features to verify that the Web Application is functioning correctly, such as checking that the correct data is displayed on a page or that the correct error messages are displayed when an error occurs.

The library is called <b>XPing365</b>, which stands for eXternal Pings, and is used to verify the availability of a server and monitor its content. 

You can find more information about the library, including documentation and examples, on the official website <a href="https://www.xping365.com">xping365.com</a>.

<!-- GETTING STARTED -->
## Getting Started

The library is distributed as a [NuGet packages](https://www.nuget.org/profiles/XPing365), which can be installed using the [.NET CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/) command `dotnet add package`. Here are the steps to get started:

### Installation using .NET CLI

1. Open a command prompt or terminal window.

2. Navigate to the directory where your project is located.

3. Run the following command to install the <b>XPing365</b> NuGet package:

   ```
   dotnet add package XPing365.Availability
   ```

4. Once the package is installed, you can start using the <b>XPing365</b> library in your project.

```c#
using XPing365.Availability.Extensions;

Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddAvailabilityTestAgent();
    });
```

```c#
using XPing365.Availability

var testAgent = _serviceProvider.GetRequiredService<AvailabilityTestAgent>();

TestSession session = await testAgent
    .RunAsync(
        new Uri("www.demoblaze.com"),
        TestSettings.DefaultForAvailability)
    .ConfigureAwait(false);
```

That’s it! You’re now ready to start automating your web application tests and monitoring your server’s content using <b>XPing365</b>.

## License

Distributed under the MIT License. See `LICENSE` file for more information.
