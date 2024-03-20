# The Testing Pipeline Explained

The TestAgent executes the XPing365 SDK’s core testing logic, runs the pipeline, and collects the results. When your TestAgent starts, it executes the components in its component container. The order of registration determines the order of execution. This order matters because each component can depend on the previous ones.

For example, the `IPAddressAccessibilityCheck` component needs the resolved IP addresses from the `DnsLookup` component. In general the validation components usually need the action components that come before them.

The figure below shows a typical testing pipeline for a web application.

<p align="center"><img src="/xping365-sdk/images/testing-pipeline.svg" onerror='this.src = "/docs/images/testing-pipeline.svg"' alt="image" width="50%" height="auto" /></p>

You can omit any of these components, but some components require others to be registered. For instance, the validation components that check the response content need the `HttpRequestSender` component. They depend on the HTTP response results from this component.

> [!NOTE] 
> The testing pipeline maintains uniform functionality across different clients, ensuring that the results are consistent whether using HttpClient or a Headless Browser. However, it should be noted that certain components may be specifically tailored to operate exclusively with one type of client.

## How to Terminate the Pipeline

Each component runs either an action or a validation test operation. If the operation fails, the component fails. In this case, the component might terminate the pipeline and stop the execution of the remaining components. The TestSession reflects all the success and failed components. You can configure this behavior in the TestSetting by enabling the `ContinueOnFailure` property. If you enable this property, all the components in the pipeline will run regardless of the state of the other components.
