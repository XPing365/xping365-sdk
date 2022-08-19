# Welcome to xping365-sdk repository

Learn about _XPing365 SDK_, a free and an open source library written in C# to help automate web applications testing.

## What's XPing 365 SDK

XPing365 SDK helps you automate web applications testing. It is designed to make eXternal Ping(s) to web applications to verify its availability and monitor its content by scraping data from the web pages. It allows users to parametrize URLs and generate hundrets of different queries. 

## Getting started with XPing 365 SDK

In this quickstart, you will learn how to:

 - Use [Dependency injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) to setup the library
 - Declare a class which will represent your web-page you're going to test
 - Fetch the web-page and write tests

## Setup the library

 This library uses dependency injection supported by the .NET framework. Please follow this link [Dependency injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) to learn more about it. This tutorial assumes you have basic knowledge about this software design pattern. 
 
 The SDK consists of two main services: [IWebDataRetriever](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/IWebDataRetriever.cs) and [IParserFactory](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/Parser/IParserFactory.cs). The first service is implemented by a concrete type [WebDataRetriever](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/WebDataRetriever.cs) and is responsible to fetch web-page content. It has a dependency to other services like `IHttpClientFactory`, `IConfiguration`, `ILogger` and [IParserFactory](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/Parser/IParserFactory.cs) which is responsible to parse the HTML content. Except [IParserFactory](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/Parser/IParserFactory.cs) all other services are provided by the .NET framework. `IConfiguration` and `ILogger` are resolved automatically, however `IHttpClientFactory` requires setup.

### Configuring IHttpClientFactory with dependency injection

 There are [multiple ways](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#multiple-ways-to-use-ihttpclientfactory) to use `IHttpClientFactory`. For the sake of brevity, this guidance uses [Named Client](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0#named-clients). 

```c#
builder.Services.AddHttpClient("httpClient", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://www.demoblaze.com/");
});
``` 

The name `httpClient` given in the above setup is provided to [WebDataRetriever](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/WebDataRetriever.cs) through `IConfiguration` service. Example of `appsettings.json` configuration fulfilling above requirement is shown below

```json
{
  "WebDataRetriever": {
    "HttpClientName": "httpClient",
    "ThrowOnError":  true
  }
}
```

It is also possible to use other options to configure `IHttpClientFactory` service. If you configure your `IHttpClientFactory` using basic setup, then no configuration is required. If you want to learn more about `WebDataRetriever` configuration file please follow the `Configuration` page of this wiki.

Once all above is configured the full setup looks like this

```c#
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
```

## Declaring web-page classes

Next step is to define classes which will represent web-pages you are going to test. Each class representing a web-page you are passing to [IWebDataRetriever](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/IWebDataRetriever.cs) service needs to derive from [HtmlSource](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/Source/HtmlSource.cs), for instance

```c#
public class HomePage : HtmlSource
{
    [XPath("//head/title", ReturnType.InnerText)]
    public string? Title { get; set; }
    
    [XPath("//a[@id='nava']/img", "src")]
    public string? LogoUrl { get; set; }
    
    [XPath("//div[@id='navbarExample']/ul")]
    public MainMenu? MainMenu { get; set; }
}
```

The `XPathAttribute` which decorates public properties guides [IParser](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/Parser/IParser.cs) how to retrieve content from the HTML source and populate the class. The class representing web-page might consist of other classes which describe other components of the web-page, i.e. MainMenu. In this case the [MainMenu](https://github.com/XPing365/xping365-sdk/blob/main/samples/SimpleTestsSample/Pages/Components/MainMenu.cs) does not to have to derive from `HtmlSource`. It's declaration is shown as follows:

```c#
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
```

In this example all the `XPathAttribute` properties are relative to the `XPathAttribute` property declared in `HomePage` class for this type.

## Web-page verification

Once the library is setup and web-page classes are declared we fetch the content of the web-page and pupulate the declared class with it based on the `XPathAttribute` decorated in public properties. First we should get the [IWebDataRetriever](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/IWebDataRetriever.cs) service, i.e.

```c#
var webDataRetriever = host.Services.GetRequiredService<IWebDataRetriever>();
```

Next we use [IWebDataRetriever](https://github.com/XPing365/xping365-sdk/blob/main/src/XPing365.Core/IWebDataRetriever.cs) API to fetch the content and populate our class as follows:

```c#
HomePage? homePage = await webDataRetriever.GetFromHtmlAsync<HomePage>("/home.html");
```

Finally we write tests against the retrieved `HomePage` class. Please refer to the `Samples` folder in this repository to check how such tests can be implemented.
