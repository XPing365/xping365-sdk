# Configuration for XPing365 SDK library

Configuration for the XPing365 SDK is delivered through `IConfiguration` service provided by .NET framework. You can learn more about [Configuration in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration). 

The following table represents keys and their corresponding values for the XPing365 SDK configuration.

| Key            | Value                     | Description                                |
|----------------|:--------------------------|:-------------------------------------------|
|HttpClientName  |"namedHttpClientName"      | Named HttpClient used during library setup |
|ThrowOnError    |True/False                 | Wether to throw on fetching/parsing error  |

Example of `appsettings.json` configuration file is shown below

```json
{
  "WebDataRetriever": {
    "HttpClientName": "httpClient",
    "ThrowOnError":  true
  }
}
```
