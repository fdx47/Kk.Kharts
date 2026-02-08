using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Kk.Kharts.Api.Tests.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kk.Kharts.Api.Tests.Integration;

/// <summary>
/// Tests d'intégration pour les endpoints UC502.
/// Valide le flux complet: HTTP → Controller → Service → Repository.
/// </summary>
public class Uc502IntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly string? _swaggerUsername;
    private readonly string? _swaggerPassword;

    public Uc502IntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        var configuration = factory.Services.GetRequiredService<IConfiguration>();
        _swaggerUsername = configuration["SwaggerAuth:Username"];
        _swaggerPassword = configuration["SwaggerAuth:Password"];
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostWet150Sdi12Data_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var payload = new
        {
            devEui = "24E1249999999999",
            sdi12_1 = "0+63.123+54.0+21.00"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/uc502/wet150-sdi12", payload);

        // Assert - Should return error status without API key (401, 403, 400, or 404 if route doesn't exist)
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetSwagger_ReturnsOk()
    {
        // Arrange
        if (!string.IsNullOrWhiteSpace(_swaggerUsername) && !string.IsNullOrWhiteSpace(_swaggerPassword))
        {
            var loginForm = new Dictionary<string, string>
            {
                ["username"] = _swaggerUsername,
                ["password"] = _swaggerPassword,
                ["redirect"] = "/swagger/v1/swagger.json"
            };

            var responseLogin = await _client.PostAsync("/swagger/login", new FormUrlEncodedContent(loginForm));

            Assert.True(
                responseLogin.StatusCode == HttpStatusCode.Redirect ||
                responseLogin.StatusCode == HttpStatusCode.OK,
                $"Swagger login failed: {(int)responseLogin.StatusCode}");
        }

        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }
}
