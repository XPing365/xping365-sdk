# Getting Started

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
using XPing365.Availability.DependencyInjection;

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

That’s it! You’re now ready to start automating your web application tests and monitoring your server’s content using <b>XPing365</b> SDK.

## Next Steps

To get started with XPing365 SDK, see the following resources:

- [Overview: How does it work?](/docs/wiki/overview.md)
- [Tutorial: Get started with XPing365 SDK](/docs/wiki/tutorial.md)
