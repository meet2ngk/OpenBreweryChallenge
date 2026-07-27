using Microsoft.EntityFrameworkCore;
using OpenBrewery.Core.Entities;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Infrastructure.Persistence.Context;

namespace OpenBrewery.Infrastructure.Persistence.Repositories
{
    public class InitializationRepository : IInitializationRepository
    {
        private readonly BreweryDbContext _context;

        public InitializationRepository(BreweryDbContext context)
        {
            _context = context;
        }

        public async Task<DatabaseInitializationStatus?> GetStatusAsync()
        {
            return await _context.DatabaseInitializationStatuses
                .FirstOrDefaultAsync();
        }

        public async Task StartAsync()
        {
            var status = await GetStatusAsync();

            if (status == null)
            {
                status = new DatabaseInitializationStatus
                {
                    IsCompleted = false,
                    LastSuccessfulPage = 0,
                    StartedAt = DateTime.UtcNow
                };

                await _context.DatabaseInitializationStatuses
                    .AddAsync(status);
            }
            else
            {
                status.IsCompleted = false;
                status.StartedAt = DateTime.UtcNow;
                status.CompletedAt = null;
            }
        }

        public async Task UpdateProgressAsync(int lastSuccessfulPage)
        {
            var status = await GetStatusAsync();

            if (status == null)
            {
                throw new InvalidOperationException(
                    "Database initialization status does not exist.");
            }

            status.LastSuccessfulPage = lastSuccessfulPage;
        }

        public async Task MarkCompletedAsync()
        {
            var status = await GetStatusAsync();

            if (status == null)
            {
                throw new InvalidOperationException(
                    "Database initialization status does not exist.");
            }

            status.IsCompleted = true;
            status.CompletedAt = DateTime.UtcNow;
        }
    }
}