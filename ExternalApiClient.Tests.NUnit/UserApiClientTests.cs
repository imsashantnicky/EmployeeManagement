using ExternalApiClient.Services;
using NUnit.Framework;
using System.Net;
using System.Text;

namespace ExternalApiClient.Tests.NUnit;

[TestFixture]
public class UserApiClientTests
{
    [Test]
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

        Assert.That(user.Id, Is.EqualTo(7));
        Assert.That(user.Name, Is.EqualTo("Asha Sharma"));
        Assert.That(user.Email, Is.EqualTo("asha@example.com"));
        Assert.That(handler.LastRequest!.Method, Is.EqualTo(HttpMethod.Get));
        Assert.That(handler.LastRequest.RequestUri!.AbsolutePath, Is.EqualTo("/users/7"));
        Assert.That(handler.RequestCount, Is.EqualTo(1));
    }

    [Test]
    public void GetUserAsync_WhenApiReturns404_ThrowsKeyNotFoundException()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var apiClient = new UserApiClient(httpClient);

        Assert.That(async () => await apiClient.GetUserAsync(42),
            Throws.TypeOf<KeyNotFoundException>());
        Assert.That(handler.RequestCount, Is.EqualTo(1));
    }

    [Test]
    public void GetUserAsync_WhenApiReturns500_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var apiClient = new UserApiClient(httpClient);

        Assert.That(async () => await apiClient.GetUserAsync(7),
            Throws.TypeOf<HttpRequestException>());
    }

    [Test]
    public void GetUserAsync_WhenIdIsInvalid_DoesNotSendRequest()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var apiClient = new UserApiClient(httpClient);

        Assert.That(async () => await apiClient.GetUserAsync(0),
            Throws.TypeOf<ArgumentException>());
        Assert.That(handler.RequestCount, Is.EqualTo(0));
    }

    [Test]
    public void GetUserAsync_WhenJsonIsMalformed_ThrowsJsonException()
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

        Assert.That(async () => await apiClient.GetUserAsync(7),
            Throws.TypeOf<System.Text.Json.JsonException>());
    }

    [Test]
    public void GetUserAsync_WhenRequestIsCancelled_ThrowsCancellationException()
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

        Assert.That(async () => await apiClient.GetUserAsync(7, cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
        Assert.That(handler.RequestCount, Is.InRange(0, 1));
    }
}
