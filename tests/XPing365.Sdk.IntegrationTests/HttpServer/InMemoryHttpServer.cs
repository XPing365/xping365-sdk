using System.Net;

namespace XPing365.Sdk.IntegrationTests.HttpServer;

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
        return Task.Run(async () =>
        {
            using HttpListener listener = new();
            listener.Prefixes.Add(GetTestServerAddress().AbsoluteUri);
            listener.Start();
            // GetContext method blocks while waiting for a request. 
            HttpListenerContext context = await listener.GetContextAsync().ConfigureAwait(false);
            // Examine client request received
            requestReceived?.Invoke(context.Request);
            // Build HttpListenerResponse
            responseBuilder(context.Response);
        }, cancellationToken);
    }

    public static Uri GetTestServerAddress()
    {
        return new Uri($"http://localhost:8080/");
    }
}
