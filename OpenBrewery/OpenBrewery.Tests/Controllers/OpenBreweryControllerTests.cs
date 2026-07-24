using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OpenBrewery.Api.Controllers;
using OpenBrewery.Core.DTOs;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Core.Models;

namespace OpenBrewery.Tests.Controllers
{
    public class OpenBreweryControllerTests
    {
        [Fact]
        public async Task Get_ShouldReturnOk_WhenBreweriesAreFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OpenBreweryController>>();
            var serviceMock = new Mock<IOpenBreweryService>();

            var breweries = new List<BreweryDto>
            {
                new BreweryDto
                {
                    Name = "Test Brewery",
                    City = "Nashik"
                }
            };

            serviceMock
                .Setup(x => x.GetBreweryAsync(It.IsAny<GetBreweriesRequest>()))
                .ReturnsAsync(breweries);

            var controller = new OpenBreweryController(
                loggerMock.Object,
                serviceMock.Object);

            var request = new GetBreweriesRequest();

            // Act
            var result = await controller.Get(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var returnedBreweries =
                Assert.IsAssignableFrom<IEnumerable<BreweryDto>>(
                    okResult.Value);

            Assert.Single(returnedBreweries);
            Assert.Equal("Test Brewery", returnedBreweries.First().Name);

            serviceMock.Verify(
                x => x.GetBreweryAsync(request),
                Times.Once);
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenNoBreweriesAreFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OpenBreweryController>>();
            var serviceMock = new Mock<IOpenBreweryService>();

            serviceMock
                .Setup(x => x.GetBreweryAsync(It.IsAny<GetBreweriesRequest>()))
                .ReturnsAsync(new List<BreweryDto>());

            var controller = new OpenBreweryController(
                loggerMock.Object,
                serviceMock.Object);

            var request = new GetBreweriesRequest();

            // Act
            var result = await controller.Get(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);

            Assert.Equal("No breweries found.", notFoundResult.Value);

            serviceMock.Verify(
                x => x.GetBreweryAsync(request),
                Times.Once);
        }

        [Fact]
        public async Task Get_ShouldReturnBadRequest_WhenServiceThrowsArgumentException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OpenBreweryController>>();
            var serviceMock = new Mock<IOpenBreweryService>();

            serviceMock
                .Setup(x => x.GetBreweryAsync(It.IsAny<GetBreweriesRequest>()))
                .ThrowsAsync(new ArgumentException("Invalid sortBy value."));

            var controller = new OpenBreweryController(
                loggerMock.Object,
                serviceMock.Object);

            var request = new GetBreweriesRequest();

            // Act
            var result = await controller.Get(request);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result.Result);

            Assert.Equal("Invalid sortBy value.", badRequestResult.Value);

            serviceMock.Verify(
                x => x.GetBreweryAsync(request),
                Times.Once);
        }

        [Fact]
        public async Task Get_ShouldReturnProblem_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<OpenBreweryController>>();
            var serviceMock = new Mock<IOpenBreweryService>();

            serviceMock
                .Setup(x => x.GetBreweryAsync(It.IsAny<GetBreweriesRequest>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            var controller = new OpenBreweryController(
                loggerMock.Object,
                serviceMock.Object);

            var request = new GetBreweriesRequest();

            // Act
            var result = await controller.Get(request);

            // Assert
            var problemResult =
                Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(
                StatusCodes.Status500InternalServerError,
                problemResult.StatusCode);

            var problemDetails =
                Assert.IsType<ProblemDetails>(problemResult.Value);

            Assert.Equal(
                "An unexpected error occured",
                problemDetails.Title);

            serviceMock.Verify(
                x => x.GetBreweryAsync(request),
                Times.Once);
        }
    }
}