using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenBrewery.Infrastructure.Persistence.Context;
using OpenBrewery.Infrastructure.Persistence.Repositories;

namespace OpenBrewery.Tests.Unit.Repositories
{
    public class InitializationRepositoryTests
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
        public async Task StartAsync_ShouldCreateStatus_WhenStatusDoesNotExist()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var repository = new InitializationRepository(context);

            // Act
            await repository.StartAsync();
            await context.SaveChangesAsync();

            // Assert
            var result = await context.DatabaseInitializationStatuses
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.False(result.IsCompleted);
            Assert.Equal(0, result.LastSuccessfulPage);
            Assert.NotNull(result.StartedAt);
            Assert.Null(result.CompletedAt);
        }

        [Fact]
        public async Task StartAsync_ShouldResetExistingStatus()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.DatabaseInitializationStatuses.Add(
                new Core.Entities.DatabaseInitializationStatus
                {
                    IsCompleted = true,
                    LastSuccessfulPage = 10,
                    StartedAt = DateTime.UtcNow.AddHours(-2),
                    CompletedAt = DateTime.UtcNow.AddHours(-1)
                });

            await context.SaveChangesAsync();

            var repository = new InitializationRepository(context);

            // Act
            await repository.StartAsync();
            await context.SaveChangesAsync();

            // Assert
            var result = await context.DatabaseInitializationStatuses
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.False(result.IsCompleted);
            Assert.Equal(10, result.LastSuccessfulPage);
            Assert.NotNull(result.StartedAt);
            Assert.Null(result.CompletedAt);
        }

        [Fact]
        public async Task UpdateProgressAsync_ShouldUpdateLastSuccessfulPage()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.DatabaseInitializationStatuses.Add(
                new Core.Entities.DatabaseInitializationStatus
                {
                    IsCompleted = false,
                    LastSuccessfulPage = 1,
                    StartedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repository = new InitializationRepository(context);

            // Act
            await repository.UpdateProgressAsync(5);
            await context.SaveChangesAsync();

            // Assert
            var result = await context.DatabaseInitializationStatuses
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.Equal(5, result.LastSuccessfulPage);
        }

        [Fact]
        public async Task UpdateProgressAsync_ShouldThrowException_WhenStatusDoesNotExist()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var repository = new InitializationRepository(context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repository.UpdateProgressAsync(5));

            Assert.Equal(
                "Database initialization status does not exist.",
                exception.Message);
        }

        [Fact]
        public async Task MarkCompletedAsync_ShouldMarkStatusAsCompleted()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.DatabaseInitializationStatuses.Add(
                new Core.Entities.DatabaseInitializationStatus
                {
                    IsCompleted = false,
                    LastSuccessfulPage = 5,
                    StartedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repository = new InitializationRepository(context);

            // Act
            await repository.MarkCompletedAsync();
            await context.SaveChangesAsync();

            // Assert
            var result = await context.DatabaseInitializationStatuses
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.True(result.IsCompleted);
            Assert.Equal(5, result.LastSuccessfulPage);
            Assert.NotNull(result.CompletedAt);
        }

        [Fact]
        public async Task MarkCompletedAsync_ShouldThrowException_WhenStatusDoesNotExist()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var repository = new InitializationRepository(context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => repository.MarkCompletedAsync());

            Assert.Equal(
                "Database initialization status does not exist.",
                exception.Message);
        }

        [Fact]
        public async Task GetStatusAsync_ShouldReturnStatus_WhenStatusExists()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            context.DatabaseInitializationStatuses.Add(
                new Core.Entities.DatabaseInitializationStatus
                {
                    IsCompleted = true,
                    LastSuccessfulPage = 10,
                    StartedAt = DateTime.UtcNow.AddHours(-2),
                    CompletedAt = DateTime.UtcNow.AddHours(-1)
                });

            await context.SaveChangesAsync();

            var repository = new InitializationRepository(context);

            // Act
            var result = await repository.GetStatusAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsCompleted);
            Assert.Equal(10, result.LastSuccessfulPage);
            Assert.NotNull(result.StartedAt);
            Assert.NotNull(result.CompletedAt);
        }

        [Fact]
        public async Task GetStatusAsync_ShouldReturnNull_WhenStatusDoesNotExist()
        {
            // Arrange
            var (context, connection) = await CreateContextAsync();
            await using var _ = connection;
            await using var __ = context;

            var repository = new InitializationRepository(context);

            // Act
            var result = await repository.GetStatusAsync();

            // Assert
            Assert.Null(result);
        }
    }
}