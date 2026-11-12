using System.Net;

namespace ExternalApiClient.Tests.NUnit;

public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken,
        Task<HttpResponseMessage>> _sendAsync;

    public int RequestCount { get; private set; }
    public HttpRequestMessage? LastRequest { get; private set; }

    public FakeHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken,
            Task<HttpResponseMessage>> sendAsync)
    {
        _sendAsync = sendAsync;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;
        LastRequest = request;
        return await _sendAsync(request, cancellationToken);
    }
}
