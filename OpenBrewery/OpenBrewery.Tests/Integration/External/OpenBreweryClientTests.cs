using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OpenBrewery.Core.Configuration;
using OpenBrewery.Infrastructure.External.Clients;
using OpenBrewery.Tests.Unit.External;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OpenBrewery.Tests.Integration.External;

public class OpenBreweryClientTests
{
    [Fact]
    public async Task GetBreweriesAsync_ShouldReturnBreweries_WhenApiReturnsSuccess()
    {
        // Arrange
        var breweries = new[]
        {
            new
            {
                name = "Test Brewery",
                city = "Pune",
                brewery_type = "micro"
            }
        };

        var json = JsonSerializer.Serialize(breweries);

        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json")
            });

        var httpClient = new HttpClient(handler);

        var loggerMock =
            new Mock<ILogger<OpenBreweryClient>>();

        var options = Options.Create(
            new OpenBreweryApiOptions
            {
                BaseUrl = "https://test-api.com/",
                BreweriesEndpoint = "breweries",
                TimeoutInSeconds = 30
            });

        var client = new OpenBreweryClient(
            loggerMock.Object,
            httpClient,
            options);

        // Act
        var result = (await client.GetBreweriesAsync()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Brewery", result[0].Name);
        Assert.Equal("Pune", result[0].City);
        Assert.Equal("micro", result[0].BreweryType);
    }
}