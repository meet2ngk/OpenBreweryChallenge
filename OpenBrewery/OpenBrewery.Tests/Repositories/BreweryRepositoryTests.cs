using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenBrewery.Core.Entities;
using OpenBrewery.Infrastructure.Persistence.Context;
using OpenBrewery.Infrastructure.Persistence.Repositories;

namespace OpenBrewery.Tests.Repositories
{
    public class BreweryRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllBreweries()
        {
            // Arrange
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<BreweryDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new BreweryDbContext(options);

            await context.Database.EnsureCreatedAsync();

            var breweries = new List<Brewery>
            {
                new Brewery
                {
                    Name = "Test Brewery 1",
                    City = "Nashik"
                },
                new Brewery
                {
                    Name = "Test Brewery 2",
                    City = "Pune"
                }
            };

            context.Breweries.AddRange(breweries);
            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Name == "Test Brewery 1");
            Assert.Contains(result, x => x.Name == "Test Brewery 2");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenDatabaseIsEmpty()
        {
            // Arrange
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<BreweryDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new BreweryDbContext(options);

            await context.Database.EnsureCreatedAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SeedAsync_ShouldAddBreweriesToDatabase()
        {
            // Arrange
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<BreweryDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new BreweryDbContext(options);

            await context.Database.EnsureCreatedAsync();

            var repository = new BreweryRepository(context);

            var breweries = new List<Brewery>
            {
                new Brewery
                {
                    Name = "Test Brewery",
                    City = "Nashik"
                }
            };

            // Act
            await repository.SeedAsync(breweries);

            // Assert
            var result = await context.Breweries.ToListAsync();

            Assert.Single(result);
            Assert.Equal("Test Brewery", result[0].Name);
            Assert.Equal("Nashik", result[0].City);
        }
    }
}