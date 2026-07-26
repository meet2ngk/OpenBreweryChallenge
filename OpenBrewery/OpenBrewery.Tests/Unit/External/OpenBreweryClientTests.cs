using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OpenBrewery.Core.Configuration;
using OpenBrewery.Infrastructure.External.Clients;
using OpenBrewery.Infrastructure.External.Models;
using System.Net;
using System.Net.Http.Json;

namespace OpenBrewery.Tests.Unit.External
{
    public class OpenBreweryClientTests
    {
        [Fact]
        public async Task GetBreweriesAsync_ShouldReturnBreweries_WhenApiCallIsSuccessful()
        {
            // Arrange
            var responseData = new List<OpenBreweryApiResponse>
            {
                new OpenBreweryApiResponse
                {
                    Name = "Test Brewery",
                    City = "Nashik",
                    BreweryType = "micro"
                }
            };

            var handler = new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(responseData)
                });

            var httpClient = new HttpClient(handler);

            var options = Options.Create(
                new OpenBreweryApiOptions
                {
                    BaseUrl = "https://api.example.com/",
                    BreweriesEndpoint = "breweries",
                    TimeoutInSeconds = 30
                });

            var loggerMock = new Mock<ILogger<OpenBreweryClient>>();

            var client = new OpenBreweryClient(
                loggerMock.Object,
                httpClient,
                options);

            // Act
            var result = await client.GetBreweriesAsync();

            // Assert
            Assert.Single(result);

            var brewery = result.First();

            Assert.Equal("Test Brewery", brewery.Name);
            Assert.Equal("Nashik", brewery.City);
            Assert.Equal("micro", brewery.BreweryType);
        }

        [Fact]
        public async Task GetBreweriesAsync_ShouldThrowHttpRequestException_WhenApiCallFails()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var httpClient = new HttpClient(handler);

            var options = Options.Create(
                new OpenBreweryApiOptions
                {
                    BaseUrl = "https://api.example.com/",
                    BreweriesEndpoint = "breweries",
                    TimeoutInSeconds = 30
                });

            var loggerMock = new Mock<ILogger<OpenBreweryClient>>();

            var client = new OpenBreweryClient(
                loggerMock.Object,
                httpClient,
                options);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(
                () => client.GetBreweriesAsync());
        }

        [Fact]
        public async Task GetBreweriesAsync_ShouldReturnEmptyCollection_WhenApiReturnsEmptyList()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(
                        new List<OpenBreweryApiResponse>())
                });

            var httpClient = new HttpClient(handler);

            var options = Options.Create(
                new OpenBreweryApiOptions
                {
                    BaseUrl = "https://api.example.com/",
                    BreweriesEndpoint = "breweries",
                    TimeoutInSeconds = 30
                });

            var loggerMock = new Mock<ILogger<OpenBreweryClient>>();

            var client = new OpenBreweryClient(
                loggerMock.Object,
                httpClient,
                options);

            // Act
            var result = await client.GetBreweriesAsync();

            // Assert
            Assert.Empty(result);
        }
    }
}