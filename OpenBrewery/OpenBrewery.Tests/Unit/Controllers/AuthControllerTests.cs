using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBrewery.Api.Controllers;
using System.IdentityModel.Tokens.Jwt;

namespace OpenBrewery.Tests.Unit.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public void GenerateToken_ShouldReturnOkWithValidJwtToken()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "ElfingOpenBreweryApiByWiproResourceNadaf@27072026",
                    ["Jwt:Issuer"] = "OpenBrewery.Api",
                    ["Jwt:Audience"] = "OpenBrewery.Client",
                    ["Jwt:Expirations"] = "30"
                })
                .Build();

            var controller = new AuthController(configuration);

            // Act
            var result = controller.GenerateToken();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var token = Assert.IsType<string>(okResult.Value);

            Assert.False(string.IsNullOrWhiteSpace(token));

            // Validate that the returned value is a valid JWT
            var handler = new JwtSecurityTokenHandler();

            Assert.True(handler.CanReadToken(token));

            var jwtToken = handler.ReadJwtToken(token);

            Assert.Equal("OpenBrewery.Api", jwtToken.Issuer);
            Assert.Contains("OpenBrewery.Client", jwtToken.Audiences);
        }

        [Fact]
        public void GenerateToken_ShouldContainReaderRole()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "ElfingOpenBreweryApiByWiproResourceNadaf@27072026",
                    ["Jwt:Issuer"] = "OpenBrewery.Api",
                    ["Jwt:Audience"] = "OpenBrewery.Client",
                    ["Jwt:Expirations"] = "30"
                })
                .Build();

            var controller = new AuthController(configuration);

            // Act
            var result = controller.GenerateToken();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var token = Assert.IsType<string>(okResult.Value);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var roleClaim = jwtToken.Claims
                .FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.Role);

            Assert.NotNull(roleClaim);
            Assert.Equal("Reader", roleClaim.Value);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenJwtConfigurationIsMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => new AuthController(configuration));

            Assert.Equal(
                "JWT configuration is missing.",
                exception.Message);
        }
    }
}