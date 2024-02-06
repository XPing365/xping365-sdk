# How does it work?
<hr/>
One of the key features of <b>XPing365</b> is its ability to check server availability through a number of action and validation steps. These steps include:
<br/>
<br/>

##### DNS lookup: 

XPing365 performs a DNS lookup to resolve the domain name of the server to its corresponding IP address. 
<br/>

##### IP address accessibility check: 

XPing365 checks whether the IP address of the server is accessible from the client machine.
<br/>

##### Send HTTP request: 

XPing365 sends an HTTP request to the server to check whether it is up and running.
<br/>

##### Response validation: 

XPing365 validates the response received from the server to ensure that it is valid and contains the expected data.
<br/>

Whenever any of these steps fail, <b>XPing365</b> makes it easy to find out the root cause of the server availability issue. This helps developers quickly identify and fix issues with their web applications.

## High Level Overview
<hr/>
Below figure provides a high-level overview of the XPing365 architecture. 
<br/><br/>

![XPing365 Architecture](/xping365-sdk/images/architecture-overview.png)

## HTTP Request System in XPing365

XPing365 uses two mechanisms to run automated tests in its HTTP request system: HttpClient and Headless Browsers.

* __HttpClient__ is a .NET class that provides a high-level abstraction for sending and receiving HTTP requests and responses. It is fast, lightweight, and easy to use. However, it does not process HTML responses or run JavaScript code, which may limit its ability to validate server responses.

* __Headless Browsers__ are browsers that run without a graphical user interface, but can still render web pages and execute JavaScript code. They are useful for simulating user interactions and testing dynamic web applications. However, they are slower, heavier, and more complex than HttpClient.

Depending on your testing needs, you can choose either or both of these mechanisms to create and run your HTTP requests with XPing365 SDK. You can also configure various parameters and options for each mechanism, such as headers, cookies, protocols, timeouts, etc. For more information, see the documentation and examples on how to use HttpClient and Headless Browsers with XPing365 SDK.

## Next Steps

To get started with <b>XPing365</b> SDK, see the following resources:

- [Tutorial: Get started with XPing365 SDK](/xping365-sdk/wiki/tutorial.html)
