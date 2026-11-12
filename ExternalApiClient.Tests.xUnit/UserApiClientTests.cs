using System.Net;
using System.Text;
using ExternalApiClient.Services;
using Xunit;

namespace ExternalApiClient.Tests.xUnit;

public class UserApiClientTests
{
    [Fact]
    public async Task GetUserAsync_WhenApiReturns200_DeserializesUser()
    {
        var handler = new FakeHttpMessageHandler((request, _) =>
        {
            var json = "{\"id\":7,\"name\":\"Asha Sharma\",\"email\":\"asha@example.com\"}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var apiClient = new UserApiClient(httpClient);

        var user = await apiClient.GetUserAsync(7);

        Assert.Equal(7, user.Id);
        Assert.Equal("Asha Sharma", user.Name);
        Assert.Equal("asha@example.com", user.Email);
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal("/users/7", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task GetUserAsync_WhenApiReturns404_ThrowsKeyNotFoundException()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var apiClient = new UserApiClient(httpClient);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => apiClient.GetUserAsync(42));

        Assert.Contains("42", ex.Message);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task GetUserAsync_WhenApiReturns500_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var apiClient = new UserApiClient(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => apiClient.GetUserAsync(7));
    }

    [Fact]
    public async Task GetUserAsync_WhenIdIsInvalid_DoesNotSendRequest()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var apiClient = new UserApiClient(httpClient);

        await Assert.ThrowsAsync<ArgumentException>(
            () => apiClient.GetUserAsync(0));

        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task GetUserAsync_WhenJsonIsMalformed_ThrowsJsonException()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid json", Encoding.UTF8, "application/json")
            }));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var apiClient = new UserApiClient(httpClient);

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            () => apiClient.GetUserAsync(7));
    }

    [Fact]
    public async Task GetUserAsync_WhenRequestIsCancelled_ThrowsCancellationException()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var apiClient = new UserApiClient(httpClient);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => apiClient.GetUserAsync(7, cts.Token));

        // HttpClient can cancel before invoking the handler.
        Assert.InRange(handler.RequestCount, 0, 1);
    }
}
