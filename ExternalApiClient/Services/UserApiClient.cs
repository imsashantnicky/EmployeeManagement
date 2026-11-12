using System.Net;
using System.Net.Http.Json;
using ExternalApiClient.Models;

namespace ExternalApiClient.Services;

public sealed class UserApiClient
{
    private readonly HttpClient _httpClient;

    public UserApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ApiUser> GetUserAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentException(
                "User ID must be greater than zero.", nameof(userId));

        using var response = await _httpClient.GetAsync(
            $"users/{userId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException(
                $"User with ID {userId} was not found.");

        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<ApiUser>(
            cancellationToken: cancellationToken);

        return user ?? throw new InvalidDataException(
            "The API returned an empty or invalid user response.");
    }
}
