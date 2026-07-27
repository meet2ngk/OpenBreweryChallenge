using Microsoft.Extensions.Logging;
using Moq;
using OpenBrewery.Core.Entities;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Infrastructure.External.Models;
using OpenBrewery.Infrastructure.Services;

namespace OpenBrewery.Tests.Unit.Services
{
    public class OpenBreweryDataSyncServiceTests
    {
        private static (
            Mock<IOpenBreweryClient> clientMock,
            Mock<IBreweryRepository> breweryRepositoryMock,
            Mock<IInitializationRepository> initializationRepositoryMock,
            Mock<IUnitOfWork> unitOfWorkMock,
            OpenBreweryDataSyncService service)
            CreateService()
        {
            var clientMock = new Mock<IOpenBreweryClient>();
            var breweryRepositoryMock = new Mock<IBreweryRepository>();
            var initializationRepositoryMock = new Mock<IInitializationRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<OpenBreweryDataSyncService>>();

            unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            var service = new OpenBreweryDataSyncService(
                clientMock.Object,
                breweryRepositoryMock.Object,
                initializationRepositoryMock.Object,
                unitOfWorkMock.Object,
                loggerMock.Object);

            return (
                clientMock,
                breweryRepositoryMock,
                initializationRepositoryMock,
                unitOfWorkMock,
                service);
        }

        [Fact]
        public async Task InitializeDatabaseAsync_ShouldSkipInitialization_WhenAlreadyCompleted()
        {
            // Arrange
            var (
                clientMock,
                breweryRepositoryMock,
                initializationRepositoryMock,
                unitOfWorkMock,
                service) = CreateService();

            initializationRepositoryMock
                .Setup(x => x.GetStatusAsync())
                .ReturnsAsync(new DatabaseInitializationStatus
                {
                    IsCompleted = true,
                    LastSuccessfulPage = 5
                });

            // Act
            await service.InitializeDatabaseAsync();

            // Assert
            initializationRepositoryMock.Verify(
                x => x.StartAsync(),
                Times.Never);

            initializationRepositoryMock.Verify(
                x => x.MarkCompletedAsync(),
                Times.Never);

            breweryRepositoryMock.Verify(
                x => x.AddRangeAsync(It.IsAny<IEnumerable<Brewery>>()),
                Times.Never);

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

            unitOfWorkMock.Verify(
                x => x.BeginTransactionAsync(),
                Times.Never);
        }

        [Fact]
        public async Task InitializeDatabaseAsync_ShouldFetchAndSaveBreweries_WhenApiReturnsData()
        {
            // Arrange
            var (
                clientMock,
                breweryRepositoryMock,
                initializationRepositoryMock,
                unitOfWorkMock,
                service) = CreateService();

            initializationRepositoryMock
                .Setup(x => x.GetStatusAsync())
                .ReturnsAsync((DatabaseInitializationStatus?)null);

            clientMock
                .SetupSequence(x => x.GetBreweriesAsync(
                    1,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null))
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
                    new OpenBreweryApiResponse
                    {
                        Name = "Test Brewery",
                        City = "Nashik",
                        Phone = "1234567890",
                        BreweryType = "micro",
                        Latitude = 19.9975,
                        Longitude = 73.7898
                    }
                })
                .ReturnsAsync(new List<OpenBreweryApiResponse>());

            // Act
            await service.InitializeDatabaseAsync();

            // Assert
            initializationRepositoryMock.Verify(
                x => x.StartAsync(),
                Times.Once);

            breweryRepositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.Is<IEnumerable<Brewery>>(breweries =>
                        breweries.Count() == 1 &&
                        breweries.First().Name == "Test Brewery" &&
                        breweries.First().City == "Nashik")),
                Times.Once);

            initializationRepositoryMock.Verify(
                x => x.UpdateProgressAsync(1),
                Times.Once);

            initializationRepositoryMock.Verify(
                x => x.MarkCompletedAsync(),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.CommitTransactionAsync(),
                Times.Exactly(2));
        }

        [Fact]
        public async Task InitializeDatabaseAsync_ShouldResumeFromNextPage_WhenPreviousPageWasSuccessful()
        {
            // Arrange
            var (
                clientMock,
                breweryRepositoryMock,
                initializationRepositoryMock,
                unitOfWorkMock,
                service) = CreateService();

            initializationRepositoryMock
                .Setup(x => x.GetStatusAsync())
                .ReturnsAsync(new DatabaseInitializationStatus
                {
                    IsCompleted = false,
                    LastSuccessfulPage = 3
                });

            clientMock
                .Setup(x => x.GetBreweriesAsync(
                    4,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null))
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
                    new OpenBreweryApiResponse
                    {
                        Name = "Resumed Brewery",
                        City = "Pune"
                    }
                });

            clientMock
                .Setup(x => x.GetBreweriesAsync(
                    5,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null))
                .ReturnsAsync(new List<OpenBreweryApiResponse>());

            // Act
            await service.InitializeDatabaseAsync();

            // Assert
            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    4,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null),
                Times.Once);

            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    1,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null),
                Times.Never);

            breweryRepositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.Is<IEnumerable<Brewery>>(breweries =>
                        breweries.Count() == 1 &&
                        breweries.First().Name == "Resumed Brewery")),
                Times.Once);

            initializationRepositoryMock.Verify(
                x => x.UpdateProgressAsync(4),
                Times.Once);

            initializationRepositoryMock.Verify(
                x => x.MarkCompletedAsync(),
                Times.Once);
        }

        [Fact]
        public async Task InitializeDatabaseAsync_ShouldProcessMultiplePages_WhenFirstPageIsFull()
        {
            // Arrange
            var (
                clientMock,
                breweryRepositoryMock,
                initializationRepositoryMock,
                unitOfWorkMock,
                service) = CreateService();

            initializationRepositoryMock
                .Setup(x => x.GetStatusAsync())
                .ReturnsAsync((DatabaseInitializationStatus?)null);

            var firstPage = Enumerable.Range(1, 200)
                .Select(x => new OpenBreweryApiResponse
                {
                    Name = $"Brewery {x}",
                    City = "Nashik"
                })
                .ToList();

            var secondPage = new List<OpenBreweryApiResponse>
            {
                new OpenBreweryApiResponse
                {
                    Name = "Last Brewery",
                    City = "Pune"
                }
            };

            clientMock
                .Setup(x => x.GetBreweriesAsync(
                    1,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null))
                .ReturnsAsync(firstPage);

            clientMock
                .Setup(x => x.GetBreweriesAsync(
                    2,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null))
                .ReturnsAsync(secondPage);

            // Act
            await service.InitializeDatabaseAsync();

            // Assert
            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    1,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null),
                Times.Once);

            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    2,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null),
                Times.Once);

            breweryRepositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.Is<IEnumerable<Brewery>>(breweries =>
                        breweries.Count() == 200)),
                Times.Once);

            breweryRepositoryMock.Verify(
                x => x.AddRangeAsync(
                    It.Is<IEnumerable<Brewery>>(breweries =>
                        breweries.Count() == 1)),
                Times.Once);

            initializationRepositoryMock.Verify(
                x => x.UpdateProgressAsync(1),
                Times.Once);

            initializationRepositoryMock.Verify(
                x => x.UpdateProgressAsync(2),
                Times.Once);

            initializationRepositoryMock.Verify(
                x => x.MarkCompletedAsync(),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.CommitTransactionAsync(),
                Times.Exactly(3));
        }

        [Fact]
        public async Task InitializeDatabaseAsync_ShouldCompleteWithoutSaving_WhenApiReturnsEmptyList()
        {
            // Arrange
            var (
                clientMock,
                breweryRepositoryMock,
                initializationRepositoryMock,
                unitOfWorkMock,
                service) = CreateService();

            initializationRepositoryMock
                .Setup(x => x.GetStatusAsync())
                .ReturnsAsync((DatabaseInitializationStatus?)null);

            clientMock
                .Setup(x => x.GetBreweriesAsync(
                    1,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null))
                .ReturnsAsync(new List<OpenBreweryApiResponse>());

            // Act
            await service.InitializeDatabaseAsync();

            // Assert
            clientMock.Verify(
                x => x.GetBreweriesAsync(
                    1,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null),
                Times.Once);

            breweryRepositoryMock.Verify(
                x => x.AddRangeAsync(It.IsAny<IEnumerable<Brewery>>()),
                Times.Never);

            initializationRepositoryMock.Verify(
                x => x.UpdateProgressAsync(It.IsAny<int>()),
                Times.Never);

            initializationRepositoryMock.Verify(
                x => x.MarkCompletedAsync(),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.BeginTransactionAsync(),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.CommitTransactionAsync(),
                Times.Once);
        }

        [Fact]
        public async Task InitializeDatabaseAsync_ShouldRollbackTransaction_WhenAddingBreweriesFails()
        {
            // Arrange
            var (
                clientMock,
                breweryRepositoryMock,
                initializationRepositoryMock,
                unitOfWorkMock,
                service) = CreateService();

            initializationRepositoryMock
                .Setup(x => x.GetStatusAsync())
                .ReturnsAsync((DatabaseInitializationStatus?)null);

            clientMock
                .Setup(x => x.GetBreweriesAsync(
                    1,
                    200,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null))
                .ReturnsAsync(new List<OpenBreweryApiResponse>
                {
                    new OpenBreweryApiResponse
                    {
                        Name = "Test Brewery",
                        City = "Nashik"
                    }
                });

            breweryRepositoryMock
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Brewery>>()))
                .ThrowsAsync(new InvalidOperationException(
                    "Database insert failed."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.InitializeDatabaseAsync());

            unitOfWorkMock.Verify(
                x => x.BeginTransactionAsync(),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.RollbackTransactionAsync(),
                Times.Once);

            unitOfWorkMock.Verify(
                x => x.CommitTransactionAsync(),
                Times.Never);

            initializationRepositoryMock.Verify(
                x => x.MarkCompletedAsync(),
                Times.Never);
        }
    }
}