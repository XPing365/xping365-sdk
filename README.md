<div id="top"></div>

![Build Status](https://github.com/XPing365/xping365-sdk/actions/workflows/dotnet.yml/badge.svg)

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h2 align="center">XPing365 SDK</h3>
  <p align="center">
    XPing365 SDK is a free and open-source .NET library written in C# to help automate web applications testing.
    <br />
    <br />
    <a href="https://github.com/XPing365/xping365-sdk/issues">Report Bug</a>
    ·
    <a href="https://github.com/XPing365/xping365-sdk/issues">Request Feature</a>
  </p>
</div>


<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#installation">Installation using .NET CLI</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details> 


<!-- ABOUT THE PROJECT -->
## About The Project

XPing365 SDK helps you automate web applications testing. It is designed to make eXternal Ping(s) to web applications to verify its availability and monitor its content by scraping data from the web pages. It allows users to parametrize URLs and generate hundrets of different queries. 

<p align="right">(<a href="#top">back to top</a>)</p>


<!-- GETTING STARTED -->
## Getting Started

[XPing365 SDK](https://www.nuget.org/packages/XPing365.Core) can be installed via the [NuGet](https://docs.nuget.org/consume/Package-Manager-Dialog) package manager or [.NET CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/). If you need help, please open an [issue](https://github.com/XPing365/xping365-sdk/issues).

### Installation using .NET CLI

   ```
   dotnet add package XPing365.Core
   ```

<p align="right">(<a href="#top">back to top</a>)</p>


<!-- USAGE EXAMPLES -->
## Usage

In this section you will find 5 steps to start working with XPing 365 SDK. It is a small subset of what's possible with XPing 365 SDK. 

-  Setup library with the [Dependency injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection).
```c#
var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices(services =>
               {
                   services.AddHttpClient<IWebDataRetriever, WebDataRetriever>("httpClient", client => 
                   {
                       client.BaseAddress = new Uri("http://example.com/");
                   });
                   services.AddTransient<IParserFactory, ParserFactory>();
                   services.AddTransient<IWebDataRetriever, WebDataRetriever>();
               })
               .Build();
```

- Define your page you're going to test
```c#
class BasicPage : HtmlSource
{
    [XPath("//head/title")]
    public string? Title { get; set; }

    [XPath("//body/h1")]
    public string? Header { get; set; }

    [XPath("//body/p")]
    public string? Paragraph { get; set; }
}
```

- Get the `WebDataRetriever` object
```c#
var webDataRetriever = host.Services.GetRequiredService<IWebDataRetriever>();
```

- Fetch the web page content
```c#
BasicPage? basicPage = await webDataRetriever.GetFromHtmlAsync<BasicPage>("/basic_page.html");
```

- Assert your web page
```c#
Assert.AreEqaul(HttpStatusCode.OK, basicPage.ResponseCode); // or alternatively check property IsSuccessResponseCode
Assert.IsTrue(basicPage.ResponseSizeInBytes < 150000);
Assert.IsTrue(basicPage.RequestEndTime - basicPage.RequestStartTime <= TimeSpan.FromSeconds(5));
Assert.AreEqual("ExampleTile", basicPage.Title);
Assert.AreEqual("ExampleHeader", basicPage.Header);
Assert.AreEqual("ExampleParagraph", basicPage.Paragraph);
```

_For more examples, please refer to the `Samples` folder in this repository._

<p align="right">(<a href="#top">back to top</a>)</p>


<!-- ROADMAP -->
## Roadmap

- [ ] Support fetching data into json format with a given set of XPath(s).
- [ ] Support XPath expressions.
- [ ] Support fetching web-pages through headless web browser.

See the [open issues](https://github.com/XPing365/xping365-sdk/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#top">back to top</a>)</p>


<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#top">back to top</a>)</p>


<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE` file for more information.

<p align="right">(<a href="#top">back to top</a>)</p>


<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* [Html Agility Pack](https://github.com/zzzprojects/html-agility-pack) XPing 365 SDK leverages the great work provided by the HAP team.

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/XPing365/xping365-sdk.svg?style=for-the-badge
[contributors-url]: https://github.com/XPing365/xping365-sdk/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/XPing365/xping365-sdk.svg?style=for-the-badge
[forks-url]: https://github.com/XPing365/xping365-sdk/network/members
[stars-shield]: https://img.shields.io/github/stars/XPing365/xping365-sdk.svg?style=for-the-badge
[stars-url]: https://github.com/XPing365/xping365-sdk/stargazers
[issues-shield]: https://img.shields.io/github/issues/XPing365/xping365-sdk.svg?style=for-the-badge
[issues-url]: https://github.com/XPing365/xping365-sdk/issues
[license-shield]: https://img.shields.io/github/license/XPing365/xping365-sdk.svg?style=for-the-badge
[license-url]: https://github.com/XPing365/xping365-sdk/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/adriandydecki
[product-screenshot]: images/screenshot.png
[Next.js]: https://img.shields.io/badge/next.js-000000?style=for-the-badge&logo=nextdotjs&logoColor=white
[Next-url]: https://nextjs.org/
[React.js]: https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB
[React-url]: https://reactjs.org/
[Vue.js]: https://img.shields.io/badge/Vue.js-35495E?style=for-the-badge&logo=vuedotjs&logoColor=4FC08D
[Vue-url]: https://vuejs.org/
[Angular.io]: https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white
[Angular-url]: https://angular.io/
[Svelte.dev]: https://img.shields.io/badge/Svelte-4A4A55?style=for-the-badge&logo=svelte&logoColor=FF3E00
[Svelte-url]: https://svelte.dev/
[Laravel.com]: https://img.shields.io/badge/Laravel-FF2D20?style=for-the-badge&logo=laravel&logoColor=white
[Laravel-url]: https://laravel.com
[Bootstrap.com]: https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white
[Bootstrap-url]: https://getbootstrap.com
[JQuery.com]: https://img.shields.io/badge/jQuery-0769AD?style=for-the-badge&logo=jquery&logoColor=white
[JQuery-url]: https://jquery.com 
