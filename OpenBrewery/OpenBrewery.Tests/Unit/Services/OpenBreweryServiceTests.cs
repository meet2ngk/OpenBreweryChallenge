using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OpenBrewery.Core.Configuration;
using OpenBrewery.Core.Enums;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Core.Models;
using OpenBrewery.Infrastructure.External.Models;
using OpenBrewery.Infrastructure.Services;

namespace OpenBrewery.Tests.Unit.Services
{
    public class OpenBreweryServiceTests
    {
        [Fact]
        public async Task GetBreweryAsync_SearchByName_ReturnsMatchingBrewery()
        {
            // Arrange
            var clientMock = new Mock<IOpenBreweryClient>();
            var repositoryMock = new Mock<IBreweryRepository>();
            var loggerMock = new Mock<ILogger<OpenBreweryService>>();

            var cache = new MemoryCache(
                new MemoryCacheOptions());

            var options = Options.Create(
                new WebApiDataSourceOptions
                {
                    DataSource = BreweryDataSource.ExternalApi
                });

            var cacheOptions = Options.Create(
                  new CacheOptions
                  {
                      ExpirationInMinutes = 10
                  });

            clientMock
                .Setup(x => x.GetBreweriesAsync())
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
                    new OpenBreweryApiResponse
                    {
                        Name = "ABC Brewery",
                        City = "Nashik",
                        Phone = "1234567890",
                        BreweryType = "micro",
                        Latitude = 19.9975,
                        Longitude = 73.7898
                    },
                    new OpenBreweryApiResponse
                    {
                        Name = "XYZ Brewery",
                        City = "Pune",
                        Phone = "9876543210",
                        BreweryType = "regional",
                        Latitude = 18.5204,
                        Longitude = 73.8567
                    }
                });

            var service = new OpenBreweryService(
                clientMock.Object,
                loggerMock.Object,
                cache,
                repositoryMock.Object,
                options,
                cacheOptions);

            var request = new GetBreweriesRequest
            {
                Search = "ABC"
            };

            // Act
            var result = await service.GetBreweryAsync(request);

            // Assert
            Assert.Single(result);
            Assert.Equal("ABC Brewery", result.First().Name);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldReturnMatchingBreweries_WhenSearchMatchesCity()
        {
            // Arrange
            var clientMock = new Mock<IOpenBreweryClient>();
            var repositoryMock = new Mock<IBreweryRepository>();
            var loggerMock = new Mock<ILogger<OpenBreweryService>>();

            var cache = new MemoryCache(new MemoryCacheOptions());

            var options = Options.Create(
                new WebApiDataSourceOptions
                {
                    DataSource = BreweryDataSource.ExternalApi
                });

            var cacheOptions = Options.Create(
                 new CacheOptions
                 {
                     ExpirationInMinutes = 10
                 });

            clientMock
                .Setup(x => x.GetBreweriesAsync())
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
            new OpenBreweryApiResponse
            {
                Name = "ABC Brewery",
                City = "Nashik"
            },
            new OpenBreweryApiResponse
            {
                Name = "XYZ Brewery",
                City = "Pune"
            }
                });

            var service = new OpenBreweryService(
                clientMock.Object,
                loggerMock.Object,
                cache,
                repositoryMock.Object,
                options,
                cacheOptions);

            var request = new GetBreweriesRequest
            {
                Search = "Nashik"
            };

            // Act
            var result = await service.GetBreweryAsync(request);

            // Assert
            Assert.Single(result);
            Assert.Equal("Nashik", result.First().City);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldSortByNameAscending()
        {
            // Arrange
            var clientMock = new Mock<IOpenBreweryClient>();
            var repositoryMock = new Mock<IBreweryRepository>();
            var loggerMock = new Mock<ILogger<OpenBreweryService>>();
            var cache = new MemoryCache(new MemoryCacheOptions());

            var options = Options.Create(
                new WebApiDataSourceOptions
                {
                    DataSource = BreweryDataSource.ExternalApi
                });

            var cacheOptions = Options.Create(
                 new CacheOptions
                 {
                     ExpirationInMinutes = 10
                 });

            clientMock
                .Setup(x => x.GetBreweriesAsync())
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
            new OpenBreweryApiResponse { Name = "Zeta Brewery" },
            new OpenBreweryApiResponse { Name = "Alpha Brewery" }
                });

            var service = new OpenBreweryService(
                clientMock.Object,
                loggerMock.Object,
                cache,
                repositoryMock.Object,
                options,
                cacheOptions);

            var request = new GetBreweriesRequest
            {
                SortBy = BrewerySortBy.Name.ToString(),
                Descending = false
            };

            // Act
            var result = (await service.GetBreweryAsync(request)).ToList();

            // Assert
            Assert.Equal("Alpha Brewery", result[0].Name);
            Assert.Equal("Zeta Brewery", result[1].Name);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldSortByNameDescending()
        {
            // Arrange
            var clientMock = new Mock<IOpenBreweryClient>();
            var repositoryMock = new Mock<IBreweryRepository>();
            var loggerMock = new Mock<ILogger<OpenBreweryService>>();
            var cache = new MemoryCache(new MemoryCacheOptions());

            var options = Options.Create(
                new WebApiDataSourceOptions
                {
                    DataSource = BreweryDataSource.ExternalApi
                });

            var cacheOptions = Options.Create(
                 new CacheOptions
                 {
                     ExpirationInMinutes = 10
                 });

            clientMock
                .Setup(x => x.GetBreweriesAsync())
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
            new OpenBreweryApiResponse { Name = "Alpha Brewery" },
            new OpenBreweryApiResponse { Name = "Zeta Brewery" }
                });

            var service = new OpenBreweryService(
                clientMock.Object,
                loggerMock.Object,
                cache,
                repositoryMock.Object,
                options,
                cacheOptions);

            var request = new GetBreweriesRequest
            {
                SortBy = BrewerySortBy.Name.ToString(),
                Descending = true
            };

            // Act
            var result = (await service.GetBreweryAsync(request)).ToList();

            // Assert
            Assert.Equal("Zeta Brewery", result[0].Name);
            Assert.Equal("Alpha Brewery", result[1].Name);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldThrowArgumentException_WhenSortByIsInvalid()
        {
            // Arrange
            var clientMock = new Mock<IOpenBreweryClient>();
            var repositoryMock = new Mock<IBreweryRepository>();
            var loggerMock = new Mock<ILogger<OpenBreweryService>>();
            var cache = new MemoryCache(new MemoryCacheOptions());

            var options = Options.Create(
                new WebApiDataSourceOptions
                {
                    DataSource = BreweryDataSource.ExternalApi
                });

            var cacheOptions = Options.Create(
                 new CacheOptions
                 {
                     ExpirationInMinutes = 10
                 });

            var service = new OpenBreweryService(
                clientMock.Object,
                loggerMock.Object,
                cache,
                repositoryMock.Object,
                options,
                cacheOptions);

            var request = new GetBreweriesRequest
            {
                SortBy = "InvalidSort"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetBreweryAsync(request));

            Assert.Contains("Invalid sortBy value", exception.Message);

            clientMock.Verify(
                x => x.GetBreweriesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldReturnBreweriesFromDatabase_WhenDatabaseHasData()
        {
            // Arrange
            var clientMock = new Mock<IOpenBreweryClient>();
            var repositoryMock = new Mock<IBreweryRepository>();
            var loggerMock = new Mock<ILogger<OpenBreweryService>>();
            var cache = new MemoryCache(new MemoryCacheOptions());

            var options = Options.Create(
                new WebApiDataSourceOptions
                {
                    DataSource = BreweryDataSource.Database
                });

            var cacheOptions = Options.Create(
                 new CacheOptions
                 {
                     ExpirationInMinutes = 10
                 });

            repositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Core.Entities.Brewery>
                {
            new Core.Entities.Brewery
            {
                Name = "Database Brewery",
                City = "Nashik",
                BreweryType = "micro"
            }
                });

            var service = new OpenBreweryService(
                clientMock.Object,
                loggerMock.Object,
                cache,
                repositoryMock.Object,
                options,
                cacheOptions);

            var request = new GetBreweriesRequest();

            // Act
            var result = (await service.GetBreweryAsync(request)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Database Brewery", result[0].Name);

            clientMock.Verify(
                x => x.GetBreweriesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldFetchAndSeedDatabase_WhenDatabaseIsEmpty()
        {
            // Arrange
            var clientMock = new Mock<IOpenBreweryClient>();
            var repositoryMock = new Mock<IBreweryRepository>();
            var loggerMock = new Mock<ILogger<OpenBreweryService>>();
            var cache = new MemoryCache(new MemoryCacheOptions());

            var options = Options.Create(
                new WebApiDataSourceOptions
                {
                    DataSource = BreweryDataSource.Database
                });

            var cacheOptions = Options.Create(
                 new CacheOptions
                 {
                     ExpirationInMinutes = 10
                 });

            repositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Core.Entities.Brewery>());

            clientMock
                .Setup(x => x.GetBreweriesAsync())
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
            new OpenBreweryApiResponse
            {
                Name = "New Brewery",
                City = "Nashik",
                BreweryType = "micro"
            }
                });

            var service = new OpenBreweryService(
                clientMock.Object,
                loggerMock.Object,
                cache,
                repositoryMock.Object,
                options, 
                cacheOptions);

            var request = new GetBreweriesRequest();

            // Act
            var result = (await service.GetBreweryAsync(request)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("New Brewery", result[0].Name);

            clientMock.Verify(
                x => x.GetBreweriesAsync(),
                Times.Once);

            repositoryMock.Verify(
                x => x.SeedAsync(
                    It.Is<IEnumerable<Core.Entities.Brewery>>(
                        breweries => breweries.Count() == 1)),
                Times.Once);
        }
    }
}