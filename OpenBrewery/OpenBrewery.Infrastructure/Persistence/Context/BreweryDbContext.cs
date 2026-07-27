using Microsoft.EntityFrameworkCore;

namespace OpenBrewery.Infrastructure.Persistence.Context
{
    public class BreweryDbContext : DbContext
    {
        public BreweryDbContext(DbContextOptions<BreweryDbContext> options) : base(options)
        {
        }

        public DbSet<Core.Entities.Brewery> Breweries => Set<Core.Entities.Brewery>();
    }
}
