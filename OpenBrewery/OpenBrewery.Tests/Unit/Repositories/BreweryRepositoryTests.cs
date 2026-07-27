using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenBrewery.Core.Entities;
using OpenBrewery.Core.Enums;
using OpenBrewery.Core.Models;
using OpenBrewery.Infrastructure.Persistence.Context;
using OpenBrewery.Infrastructure.Persistence.Repositories;

namespace OpenBrewery.Tests.Unit.Repositories
{
    public class BreweryRepositoryTests
    {
        private static async Task<(BreweryDbContext Context, SqliteConnection Connection)> CreateContextAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<BreweryDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new BreweryDbContext(options);
            await context.Database.EnsureCreatedAsync();

            return (context, connection);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllBreweries()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery
                {
                    Name = "Test Brewery 1",
                    City = "Nashik"
                },
                new Brewery
                {
                    Name = "Test Brewery 2",
                    City = "Pune"
                });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync(
                new BreweryQuery
                {
                    PageNumber = 1,
                    PageSize = 200
                });

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Name == "Test Brewery 1");
            Assert.Contains(result, x => x.Name == "Test Brewery 2");
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByName()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery
                {
                    Name = "ABC Brewery",
                    City = "Nashik"
                },
                new Brewery
                {
                    Name = "XYZ Brewery",
                    City = "Pune"
                });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync(
                new BreweryQuery
                {
                    Search = "ABC",
                    SearchBy = "name",
                    PageNumber = 1,
                    PageSize = 200
                });

            // Assert
            Assert.Single(result);
            Assert.Equal("ABC Brewery", result[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByCity()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery
                {
                    Name = "ABC Brewery",
                    City = "Nashik"
                },
                new Brewery
                {
                    Name = "XYZ Brewery",
                    City = "Pune"
                });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync(
                new BreweryQuery
                {
                    Search = "Nashik",
                    SearchBy = "city",
                    PageNumber = 1,
                    PageSize = 200
                });

            // Assert
            Assert.Single(result);
            Assert.Equal("Nashik", result[0].City);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllBreweries_WhenSearchByIsInvalid()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery
                {
                    Name = "ABC Brewery",
                    City = "Nashik"
                },
                new Brewery
                {
                    Name = "XYZ Brewery",
                    City = "Pune"
                });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync(
                new BreweryQuery
                {
                    Search = "ABC",
                    SearchBy = "invalid",
                    PageNumber = 1,
                    PageSize = 200
                });

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByNameAscending()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery
                {
                    Name = "Zeta Brewery",
                    City = "Pune"
                },
                new Brewery
                {
                    Name = "Alpha Brewery",
                    City = "Nashik"
                });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync(
                new BreweryQuery
                {
                    SortBy = BrewerySortBy.Name,
                    Descending = false,
                    PageNumber = 1,
                    PageSize = 200
                });

            // Assert
            Assert.Equal("Alpha Brewery", result[0].Name);
            Assert.Equal("Zeta Brewery", result[1].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByNameDescending()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery
                {
                    Name = "Alpha Brewery",
                    City = "Nashik"
                },
                new Brewery
                {
                    Name = "Zeta Brewery",
                    City = "Pune"
                });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync(
                new BreweryQuery
                {
                    SortBy = BrewerySortBy.Name,
                    Descending = true,
                    PageNumber = 1,
                    PageSize = 200
                });

            // Assert
            Assert.Equal("Zeta Brewery", result[0].Name);
            Assert.Equal("Alpha Brewery", result[1].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByCityAscending()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery
                {
                    Name = "Pune Brewery",
                    City = "Pune"
                },
                new Brewery
                {
                    Name = "Nashik Brewery",
                    City = "Nashik"
                });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync(
                new BreweryQuery
                {
                    SortBy = BrewerySortBy.City,
                    Descending = false,
                    PageNumber = 1,
                    PageSize = 200
                });

            // Assert
            Assert.Equal("Nashik", result[0].City);
            Assert.Equal("Pune", result[1].City);
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByCityDescending()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery
                {
                    Name = "Nashik Brewery",
                    City = "Nashik"
                },
                new Brewery
                {
                    Name = "Pune Brewery",
                    City = "Pune"
                });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync(
                new BreweryQuery
                {
                    SortBy = BrewerySortBy.City,
                    Descending = true,
                    PageNumber = 1,
                    PageSize = 200
                });

            // Assert
            Assert.Equal("Pune", result[0].City);
            Assert.Equal("Nashik", result[1].City);
        }

        [Fact]
        public async Task GetAllAsync_ShouldApplyPagination()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery { Name = "Brewery 1", City = "Nashik" },
                new Brewery { Name = "Brewery 2", City = "Pune" },
                new Brewery { Name = "Brewery 3", City = "Mumbai" });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync(
                new BreweryQuery
                {
                    PageNumber = 2,
                    PageSize = 1
                });

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenDatabaseIsEmpty()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetAllAsync(
                new BreweryQuery
                {
                    PageNumber = 1,
                    PageSize = 200
                });

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddRangeAsync_ShouldAddBreweriesToDatabase()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var repository = new BreweryRepository(context);

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

            // Act
            await repository.AddRangeAsync(breweries);
            await context.SaveChangesAsync();

            // Assert
            var result = await context.Breweries.ToListAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Name == "Test Brewery 1");
            Assert.Contains(result, x => x.Name == "Test Brewery 2");
        }

        [Fact]
        public async Task GetForDistanceAsync_ShouldReturnMatchingBreweries_WhenSearchMatchesNameOrCity()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery
                {
                    Name = "ABC Brewery",
                    City = "Nashik"
                },
                new Brewery
                {
                    Name = "XYZ Brewery",
                    City = "Pune"
                });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetForDistanceAsync("Nashik");

            // Assert
            Assert.Single(result);
            Assert.Equal("ABC Brewery", result[0].Name);
        }

        [Fact]
        public async Task GetForDistanceAsync_ShouldReturnAllBreweries_WhenSearchIsEmpty()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.Breweries.AddRange(
                new Brewery
                {
                    Name = "ABC Brewery",
                    City = "Nashik"
                },
                new Brewery
                {
                    Name = "XYZ Brewery",
                    City = "Pune"
                });

            await context.SaveChangesAsync();

            var repository = new BreweryRepository(context);

            // Act
            var result = await repository.GetForDistanceAsync(null);

            // Assert
            Assert.Equal(2, result.Count);
        }
    }
}