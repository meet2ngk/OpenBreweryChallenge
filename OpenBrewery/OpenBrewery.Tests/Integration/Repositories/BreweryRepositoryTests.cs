using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenBrewery.Core.Entities;
using OpenBrewery.Core.Models;
using OpenBrewery.Infrastructure.Persistence.Context;
using OpenBrewery.Infrastructure.Persistence.Repositories;

namespace OpenBrewery.Tests.Integration.Repositories
{
    public class BreweryRepositoryTests
    {

        [Fact]
        public async Task GetAllAsync_ShouldReturnBreweriesFromDatabase()
        {
            //arrange
            SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
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
                    Name = "Brewery One",
                    City = "Pune",
                    BreweryType = "micro"
                },
                new Brewery
                {
                    Name = "Brewery Two",
                    City = "Mumbai",
                    BreweryType = "brewpub"
                }

            };
            await repository.AddRangeAsync(breweries);
            await context.SaveChangesAsync();
            //act
            var result = await context.Breweries.ToListAsync();

            //assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Name == "Brewery One");
            Assert.Contains(result, x => x.Name == "Brewery Two");
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByName_WhenSearchByIsName()
        {
            // Arrange
            await using var connection =
                new SqliteConnection("DataSource=:memory:");

            await connection.OpenAsync();

            var options =
                new DbContextOptionsBuilder<BreweryDbContext>()
                    .UseSqlite(connection)
                    .Options;

            await using var context =
                new BreweryDbContext(options);

            await context.Database.EnsureCreatedAsync();

            var breweries = new List<Brewery>
                {
                    new Brewery
                    {
                        Name = "ABC Brewery",
                        City = "Nashik"
                    },
                    new Brewery
                    {
                        Name = "XYZ Brewery",
                        City = "Pune"
                    }
                };

            context.Breweries.AddRange(breweries);
            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            var query = new BreweryQuery
            {
                Search = "ABC",
                SearchBy = "name",
                PageNumber = 1,
                PageSize = 200
            };

            // Act
            var result = await repository.GetAllAsync(query);

            // Assert
            Assert.Single(result);
            Assert.Equal("ABC Brewery", result[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByCity_WhenSearchByIsCity()
        {
            // Arrange
            await using var connection =
                new SqliteConnection("DataSource=:memory:");

            await connection.OpenAsync();

            var options =
                new DbContextOptionsBuilder<BreweryDbContext>()
                    .UseSqlite(connection)
                    .Options;

            await using var context =
                new BreweryDbContext(options);

            await context.Database.EnsureCreatedAsync();

            var breweries = new List<Brewery>
                {
                    new Brewery
                    {
                        Name = "ABC Brewery",
                        City = "Nashik"
                    },
                    new Brewery
                    {
                        Name = "XYZ Brewery",
                        City = "Pune"
                    }
                };

            context.Breweries.AddRange(breweries);
            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            var query = new BreweryQuery
            {
                Search = "Nashik",
                SearchBy = "city",
                PageNumber = 1,
                PageSize = 200
            };

            // Act
            var result = await repository.GetAllAsync(query);

            // Assert
            Assert.Single(result);
            Assert.Equal("Nashik", result[0].City);
        }
    }
}
