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
        private static (
            Mock<IOpenBreweryClient> clientMock,
            Mock<IBreweryRepository> repositoryMock,
            OpenBreweryService service)
            CreateService(BreweryDataSource dataSource)
        {
            var clientMock = new Mock<IOpenBreweryClient>();
            var repositoryMock = new Mock<IBreweryRepository>();
            var loggerMock = new Mock<ILogger<OpenBreweryService>>();

            var cache = new MemoryCache(
                new MemoryCacheOptions());

            var options = Options.Create(
                new WebApiDataSourceOptions
                {
                    DataSource = dataSource
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

            return (
                clientMock,
                repositoryMock,
                service);
        }

        [Fact]
        public async Task GetBreweryAsync_SearchByName_ShouldPassNameSearchToExternalApi()
        {
            // Arrange
            var (
                clientMock,
                _,
                service) = CreateService(
                    BreweryDataSource.ExternalApi);

            clientMock
                .Setup(x => x.GetBreweriesAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<double?>(),
                    It.IsAny<double?>()))
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
                    new OpenBreweryApiResponse
                    {
                        Name = "ABC Brewery",
                        City = "Nashik"
                    }
                });

            var request = new GetBreweriesRequest
            {
                Search = "ABC",
                SearchBy = "name"
            };

            // Act
            var result = await service.GetBreweryAsync(request);

            // Assert
            Assert.Single(result);
            Assert.Equal("ABC Brewery", result.First().Name);

            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    1,
                    20,
                    "ABC",
                    "name",
                    null,
                    false,
                    null,
                    null),
                Times.Once);
        }

        [Fact]
        public async Task GetBreweryAsync_SearchByCity_ShouldPassCitySearchToExternalApi()
        {
            // Arrange
            var (
                clientMock,
                _,
                service) = CreateService(
                    BreweryDataSource.ExternalApi);

            clientMock
                .Setup(x => x.GetBreweriesAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<double?>(),
                    It.IsAny<double?>()))
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
                    new OpenBreweryApiResponse
                    {
                        Name = "ABC Brewery",
                        City = "Nashik"
                    }
                });

            var request = new GetBreweriesRequest
            {
                Search = "Nashik",
                SearchBy = "city"
            };

            // Act
            var result = await service.GetBreweryAsync(request);

            // Assert
            Assert.Single(result);
            Assert.Equal("Nashik", result.First().City);

            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    1,
                    20,
                    "Nashik",
                    "city",
                    null,
                    false,
                    null,
                    null),
                Times.Once);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldPassSortByNameAscendingToExternalApi()
        {
            // Arrange
            var (
                clientMock,
                _,
                service) = CreateService(
                    BreweryDataSource.ExternalApi);

            clientMock
                .Setup(x => x.GetBreweriesAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<double?>(),
                    It.IsAny<double?>()))
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
                    new OpenBreweryApiResponse
                    {
                        Name = "Alpha Brewery"
                    },
                    new OpenBreweryApiResponse
                    {
                        Name = "Zeta Brewery"
                    }
                });

            var request = new GetBreweriesRequest
            {
                SortBy = BrewerySortBy.Name.ToString(),
                Descending = false
            };

            // Act
            var result =
                (await service.GetBreweryAsync(request)).ToList();

            // Assert
            Assert.Equal("Alpha Brewery", result[0].Name);
            Assert.Equal("Zeta Brewery", result[1].Name);

            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    1,
                    20,
                    null,
                    null,
                    "Name",
                    false,
                    null,
                    null),
                Times.Once);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldPassSortByNameDescendingToExternalApi()
        {
            // Arrange
            var (
                clientMock,
                _,
                service) = CreateService(
                    BreweryDataSource.ExternalApi);

            clientMock
                .Setup(x => x.GetBreweriesAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<double?>(),
                    It.IsAny<double?>()))
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
                    new OpenBreweryApiResponse
                    {
                        Name = "Zeta Brewery"
                    },
                    new OpenBreweryApiResponse
                    {
                        Name = "Alpha Brewery"
                    }
                });

            var request = new GetBreweriesRequest
            {
                SortBy = BrewerySortBy.Name.ToString(),
                Descending = true
            };

            // Act
            var result =
                (await service.GetBreweryAsync(request)).ToList();

            // Assert
            Assert.Equal("Zeta Brewery", result[0].Name);
            Assert.Equal("Alpha Brewery", result[1].Name);

            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    1,
                    20,
                    null,
                    null,
                    "Name",
                    true,
                    null,
                    null),
                Times.Once);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldThrowArgumentException_WhenSortByIsInvalid()
        {
            // Arrange
            var (
                clientMock,
                _,
                service) = CreateService(
                    BreweryDataSource.ExternalApi);

            var request = new GetBreweriesRequest
            {
                SortBy = "InvalidSort"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetBreweryAsync(request));

            Assert.Contains(
                "Invalid sortBy value",
                exception.Message);

            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<double?>(),
                    It.IsAny<double?>()),
                Times.Never);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldReturnBreweriesFromDatabase_WhenDatabaseHasData()
        {
            // Arrange
            var (
                clientMock,
                repositoryMock,
                service) = CreateService(
                    BreweryDataSource.Database);

            repositoryMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<BreweryQuery>()))
                .ReturnsAsync(new List<Core.Entities.Brewery>
                {
                    new Core.Entities.Brewery
                    {
                        Name = "Database Brewery",
                        City = "Nashik",
                        BreweryType = "micro"
                    }
                });

            var request = new GetBreweriesRequest();

            // Act
            var result =
                (await service.GetBreweryAsync(request)).ToList();

            // Assert
            Assert.Single(result);

            Assert.Equal(
                "Database Brewery",
                result[0].Name);

            repositoryMock.Verify(
                x => x.GetAllAsync(
                    It.IsAny<BreweryQuery>()),
                Times.Once);

            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<double?>(),
                    It.IsAny<double?>()),
                Times.Never);
        }

        [Fact]
        public async Task GetBreweryAsync_ShouldReturnEmptyList_WhenDatabaseIsEmpty()
        {
            // Arrange
            var (
                clientMock,
                repositoryMock,
                service) = CreateService(
                    BreweryDataSource.Database);

            repositoryMock
                .Setup(x => x.GetAllAsync(
                    It.IsAny<BreweryQuery>()))
                .ReturnsAsync(new List<Core.Entities.Brewery>());

            var request = new GetBreweriesRequest();

            // Act
            var result =
                (await service.GetBreweryAsync(request)).ToList();

            // Assert
            Assert.Empty(result);

            repositoryMock.Verify(
                x => x.GetAllAsync(
                    It.IsAny<BreweryQuery>()),
                Times.Once);

            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<double?>(),
                    It.IsAny<double?>()),
                Times.Never);
        }
    }
}