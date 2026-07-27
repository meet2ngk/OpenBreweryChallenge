using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenBrewery.Infrastructure.Persistence;
using OpenBrewery.Infrastructure.Persistence.Context;

namespace OpenBrewery.Tests.Unit.Persistence
{
    public class UnitOfWorkTests
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
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var unitOfWork = new UnitOfWork(context);

            context.Breweries.Add(
                new Core.Entities.Brewery
                {
                    Name = "Test Brewery",
                    City = "Nashik"
                });

            // Act
            var result = await unitOfWork.SaveChangesAsync();

            // Assert
            Assert.Equal(1, result);

            var brewery = await context.Breweries
                .FirstOrDefaultAsync();

            Assert.NotNull(brewery);
            Assert.Equal("Test Brewery", brewery.Name);
            Assert.Equal("Nashik", brewery.City);
        }

        [Fact]
        public async Task BeginTransactionAsync_ShouldStartTransaction()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var unitOfWork = new UnitOfWork(context);

            // Act
            await unitOfWork.BeginTransactionAsync();

            // Assert
            Assert.NotNull(context.Database.CurrentTransaction);

            await unitOfWork.RollbackTransactionAsync();
        }

        [Fact]
        public async Task CommitTransactionAsync_ShouldCommitChanges()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var unitOfWork = new UnitOfWork(context);

            await unitOfWork.BeginTransactionAsync();

            context.Breweries.Add(
                new Core.Entities.Brewery
                {
                    Name = "Committed Brewery",
                    City = "Nashik"
                });

            await unitOfWork.SaveChangesAsync();

            // Act
            await unitOfWork.CommitTransactionAsync();

            // Assert
            Assert.Null(context.Database.CurrentTransaction);

            var brewery = await context.Breweries
                .FirstOrDefaultAsync();

            Assert.NotNull(brewery);
            Assert.Equal("Committed Brewery", brewery.Name);
        }

        [Fact]
        public async Task CommitTransactionAsync_ShouldThrowException_WhenNoActiveTransactionExists()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var unitOfWork = new UnitOfWork(context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => unitOfWork.CommitTransactionAsync());

            Assert.Equal(
                "No active transaction exists.",
                exception.Message);
        }

        [Fact]
        public async Task RollbackTransactionAsync_ShouldRollbackChanges()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var unitOfWork = new UnitOfWork(context);

            await unitOfWork.BeginTransactionAsync();

            context.Breweries.Add(
                new Core.Entities.Brewery
                {
                    Name = "Rollback Brewery",
                    City = "Pune"
                });

            await unitOfWork.SaveChangesAsync();

            // Act
            await unitOfWork.RollbackTransactionAsync();

            // Assert
            Assert.Null(context.Database.CurrentTransaction);

            await using var verificationContext =
                new BreweryDbContext(
                    new DbContextOptionsBuilder<BreweryDbContext>()
                        .UseSqlite(connection)
                        .Options);

            var brewery = await verificationContext.Breweries
                .FirstOrDefaultAsync();

            Assert.Null(brewery);
        }

        [Fact]
        public async Task RollbackTransactionAsync_ShouldDoNothing_WhenNoActiveTransactionExists()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var unitOfWork = new UnitOfWork(context);

            // Act
            var exception = await Record.ExceptionAsync(
                () => unitOfWork.RollbackTransactionAsync());

            // Assert
            Assert.Null(exception);
            Assert.Null(context.Database.CurrentTransaction);
        }
    }
}