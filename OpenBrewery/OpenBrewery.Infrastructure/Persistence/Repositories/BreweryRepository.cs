using Microsoft.EntityFrameworkCore;
using OpenBrewery.Core.Entities;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Infrastructure.Persistence.Context;

namespace OpenBrewery.Infrastructure.Persistence.Repositories
{
    public class BreweryRepository : IBreweryRepository
    {
        private readonly BreweryDbContext _context;

        public BreweryRepository(BreweryDbContext context)
        {
            _context = context;
        }
        public async Task<List<Brewery>> GetAllAsync()
        {
            return await _context.Breweries.AsNoTracking().ToListAsync();
        }

        public async Task SeedAsync(IEnumerable<Brewery> breweries)
        {
            await _context.Breweries.AddRangeAsync(breweries);

            await _context.SaveChangesAsync();
        }
    }
}
