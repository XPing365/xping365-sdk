# How does it work?
<hr/>
One of the key features of <b>XPing365</b> is its ability to check server availability and the data it returns through a number of action and validation steps. These steps include:
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

XPing365 validates the response received from the server to ensure that it is valid and contains the expected data. This validation process involves checking various aspects of the response, such as the HTTP status code, the HTTP headers, and the content. By validating the response, XPing365 can verify that the server is functioning correctly and that the response matches the expected criteria.

The response validation happens on different layers, depending on the type and level of validation required. For example, XPing365 can perform the following types of validation:

* __HTTP validation__: This layer checks the HTTP protocol aspects of the response, such as the status code, the headers, and the cookies. This can help to ensure that the server is responding with the correct HTTP status (e.g. 200 OK, 404 Not Found, etc.), that the headers are consistent with the request and the content type, and that the cookies are set or updated as expected.
* __Content validation__: This layer checks the content of the response, such as the HTML, JSON, XML, or plain text. This can help to ensure that the response contains the expected data, such as the correct elements, attributes, values, or keywords. XPing365 can use various methods to validate the content, such as XPath, regular expressions, or custom validators.

<br/>

Whenever any of the above steps fail, __XPing365__ makes it easy to find out the root cause of the server issue. This helps developers quickly identify and fix issues with their web applications.

## High Level Overview
<hr/>
Below figure provides a high-level overview of the <b>XPing365</b> architecture. 
<br/><br/>

![XPing365 Architecture](/xping365-sdk/images/architecture-overview.svg)

## HTTP Request System

XPing365 uses two mechanisms to run automated tests in its HTTP request system: HttpClient and Headless Browsers.

* __HttpClient__ is a .NET class that provides a high-level abstraction for sending and receiving HTTP requests and responses. It is fast, lightweight, and easy to use. However, it does not process HTML responses or run JavaScript code, which may limit its ability to validate server responses.

* __Headless Browsers__ are browsers that run without a graphical user interface, but can still render web pages and execute JavaScript code. They are useful for simulating user interactions and testing dynamic web applications. However, they are slower, heavier, and more complex than HttpClient.

Depending on your testing needs, you can choose either or both of these mechanisms to create and run your HTTP requests with XPing365 SDK. You can also configure various parameters and options for each mechanism, such as headers, cookies, protocols, timeouts, etc. For more information, see the documentation and examples on how to use HttpClient and Headless Browsers with XPing365 SDK.

## Test Session Serialization System

XPing365 test session serialization is a feature that allows users to convert test sessions into a format that can be stored or transmitted. A test session is a class that represents a test execution and its attributes. These include a state, a start date, and a duration that indicate the overall status of the test session, such as completed, failed, or declined. A test session can store various data related to the test operation, such as resolved IP addresses from DNS lookup, HTTP response headers, HTML content, and captured screenshots from the headless browsers. Test session serialization has several benefits, such as:

* Saving and loading test sessions for further analysis and comparison. This can help users to evaluate the performance and quality of their testing activities, as well as to identify the areas that need more testing or improvement.
* Transferring test sessions between different machines or applications. This can help users to share their testing information with other users or tools, as well as to run their tests on different environments or platforms.
* Simplifying the implementation of other features that might require serialization, such as session replication, backup, or migration. This can help users to ensure the reliability and consistency of their testing data, as well as to avoid data loss or corruption.

XPing365 test session serialization supports two formats: binary and XML. Users can choose the format that suits their needs and preferences, depending on the size, readability, and compatibility of the data. Users can use the TestSessionSerializer class, which provides two methods to serialize and deserialize test sessions.

XPing365 test session serialization is a powerful and useful feature that enhances the functionality and usability of the XPing365 project. It enables users to store, transfer, and manipulate test sessions in a convenient and efficient way.