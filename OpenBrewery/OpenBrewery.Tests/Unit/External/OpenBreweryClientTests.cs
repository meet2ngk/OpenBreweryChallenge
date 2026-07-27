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

            var options = Options.Create(new OpenBreweryApiOptions
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
            var result = await client.GetBreweriesAsync(1, 200);

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

            var options = Options.Create(new OpenBreweryApiOptions
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
                () => client.GetBreweriesAsync(1, 200));
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

            var options = Options.Create(new OpenBreweryApiOptions
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
            var result = await client.GetBreweriesAsync(1, 200);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBreweriesAsync_ShouldSendByName_WhenSearchByIsName()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(
                        new List<OpenBreweryApiResponse>())
                });

            var httpClient = new HttpClient(handler);

            var options = Options.Create(new OpenBreweryApiOptions
            {
                BaseUrl = "https://api.example.com/",
                BreweriesEndpoint = "breweries",
                TimeoutInSeconds = 30
            });

            var client = new OpenBreweryClient(
                new Mock<ILogger<OpenBreweryClient>>().Object,
                httpClient,
                options);

            // Act
            await client.GetBreweriesAsync(
                1,
                200,
                "ABC",
                "name");

            // Assert
            Assert.NotNull(handler.RequestUri);
            Assert.Contains("by_name=ABC", handler.RequestUri.Query);
            Assert.DoesNotContain("by_city", handler.RequestUri.Query);
            Assert.DoesNotContain("by_dist", handler.RequestUri.Query);
        }

        [Fact]
        public async Task GetBreweriesAsync_ShouldSendByCity_WhenSearchByIsCity()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(
                        new List<OpenBreweryApiResponse>())
                });

            var httpClient = new HttpClient(handler);

            var options = Options.Create(new OpenBreweryApiOptions
            {
                BaseUrl = "https://api.example.com/",
                BreweriesEndpoint = "breweries",
                TimeoutInSeconds = 30
            });

            var client = new OpenBreweryClient(
                new Mock<ILogger<OpenBreweryClient>>().Object,
                httpClient,
                options);

            // Act
            await client.GetBreweriesAsync(
                1,
                200,
                "Nashik",
                "city");

            // Assert
            Assert.NotNull(handler.RequestUri);
            Assert.Contains("by_city=Nashik", handler.RequestUri.Query);
            Assert.DoesNotContain("by_name", handler.RequestUri.Query);
            Assert.DoesNotContain("by_dist", handler.RequestUri.Query);
        }
        

        [Fact]
        public async Task GetBreweriesAsync_ShouldSendNameAscendingSort_WhenSortByIsName()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(
                        new List<OpenBreweryApiResponse>())
                });

            var httpClient = new HttpClient(handler);

            var options = Options.Create(new OpenBreweryApiOptions
            {
                BaseUrl = "https://api.example.com/",
                BreweriesEndpoint = "breweries",
                TimeoutInSeconds = 30
            });

            var client = new OpenBreweryClient(
                new Mock<ILogger<OpenBreweryClient>>().Object,
                httpClient,
                options);

            // Act
            await client.GetBreweriesAsync(
                1,
                200,
                null,
                null,
                "name",
                false);

            // Assert
            Assert.NotNull(handler.RequestUri);
            Assert.Contains("name.asc", handler.RequestUri.Query);
        }

        [Fact]
        public async Task GetBreweriesAsync_ShouldSendCityDescendingSort_WhenSortByIsCity()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(
                        new List<OpenBreweryApiResponse>())
                });

            var httpClient = new HttpClient(handler);

            var options = Options.Create(new OpenBreweryApiOptions
            {
                BaseUrl = "https://api.example.com/",
                BreweriesEndpoint = "breweries",
                TimeoutInSeconds = 30
            });

            var client = new OpenBreweryClient(
                new Mock<ILogger<OpenBreweryClient>>().Object,
                httpClient,
                options);

            // Act
            await client.GetBreweriesAsync(
                1,
                200,
                null,
                null,
                "city",
                true);

            // Assert
            Assert.NotNull(handler.RequestUri);
            Assert.Contains("city.desc", handler.RequestUri.Query);
        }

        [Fact]
        public async Task GetBreweriesAsync_ShouldNotSendDistanceSortToExternalApi_WhenSortByIsDistance()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(
                        new List<OpenBreweryApiResponse>())
                });

            var httpClient = new HttpClient(handler);

            var options = Options.Create(new OpenBreweryApiOptions
            {
                BaseUrl = "https://api.example.com/",
                BreweriesEndpoint = "breweries",
                TimeoutInSeconds = 30
            });

            var client = new OpenBreweryClient(
                new Mock<ILogger<OpenBreweryClient>>().Object,
                httpClient,
                options);

            // Act
            await client.GetBreweriesAsync(
                1,
                200,
                null,
                null,
                "distance",
                false,
                19.9975,
                73.7898);

            // Assert
            Assert.NotNull(handler.RequestUri);
            Assert.DoesNotContain("distance", handler.RequestUri.Query);
            Assert.DoesNotContain("distance.asc", handler.RequestUri.Query);
            Assert.DoesNotContain("distance.desc", handler.RequestUri.Query);
        }
    }
}