using System.Net;

namespace XPing365.Sdk.IntegrationTests.HttpServer;

internal static class InMemoryHttpServer
{
    public static Task TestServer(
        Action<HttpListenerResponse> responseBuilder)
    {
        return TestServer(responseBuilder, requestReceived: (request) => { });
    }

    public static Task TestServer(
        Action<HttpListenerResponse> responseBuilder,
        Action<HttpListenerRequest> requestReceived)
    {
        return Task.Run(() =>
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
        });
    }

    public static Uri GetTestServerAddress()
    {
        return new Uri($"http://localhost:8080/");
    }
}
