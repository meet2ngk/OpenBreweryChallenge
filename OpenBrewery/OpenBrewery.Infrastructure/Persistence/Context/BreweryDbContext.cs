using Microsoft.EntityFrameworkCore;
using OpenBrewery.Core.Entities;

namespace OpenBrewery.Infrastructure.Persistence.Context
{
    public class BreweryDbContext : DbContext
    {
        public BreweryDbContext(
            DbContextOptions<BreweryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Brewery> Breweries
            => Set<Brewery>();

        public DbSet<DatabaseInitializationStatus>
            DatabaseInitializationStatuses
            => Set<DatabaseInitializationStatus>();
    }
}