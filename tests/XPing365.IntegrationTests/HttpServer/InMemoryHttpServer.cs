using System.Net;

namespace XPing365.IntegrationTests.HttpServer;

internal static class InMemoryHttpServer
{
    public static Task TestServer(
        Action<HttpListenerResponse> responseBuilder,
        CancellationToken cancellationToken = default)
    {
        return TestServer(responseBuilder, requestReceived: (request) => { }, cancellationToken);
    }

    public static Task TestServer(
        Action<HttpListenerResponse> responseBuilder,
        Action<HttpListenerRequest> requestReceived,
        CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(() =>
        {
            using HttpListener listener = new();
            listener.Prefixes.Add(GetTestServerAddress().AbsoluteUri);
            listener.Start();
            // GetContext method blocks while waiting for a request. 
            HttpListenerContext context = listener.GetContext();
            // Examine client request received
            requestReceived?.Invoke(context.Request);
            // Build HttpListenerResponse
            responseBuilder(context.Response);
            // Wait for the receiver of the response to read the stream.
            // Cancelled token exits the Task.Delay without having to wait longer than is necessary.
            Task.Delay(millisecondsDelay: 1000, cancellationToken);
        }, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
    }

    public static Uri GetTestServerAddress()
    {
        return new Uri($"http://localhost:8080/");
    }
}
