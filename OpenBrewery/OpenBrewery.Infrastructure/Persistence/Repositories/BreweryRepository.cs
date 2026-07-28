using Microsoft.EntityFrameworkCore;
using OpenBrewery.Core.Entities;
using OpenBrewery.Core.Enums;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Core.Models;
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
        public async Task<List<Brewery>> GetAllAsync(BreweryQuery query)
        {
            IQueryable<Brewery> breweries = _context.Breweries
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Search) &&
                !string.IsNullOrWhiteSpace(query.SearchBy))
            {
                switch (query.SearchBy.ToLowerInvariant())
                {
                    case "name":
                        breweries = breweries.Where(x =>
                            x.Name != null &&
                            x.Name.Contains(query.Search));
                        break;

                    case "city":
                        breweries = breweries.Where(x =>
                            x.City != null &&
                            x.City.Contains(query.Search));
                        break;
                }
            }

            switch (query.SortBy)
            {
                case BrewerySortBy.Name:
                    breweries = query.Descending
                        ? breweries.OrderByDescending(x => x.Name)
                        : breweries.OrderBy(x => x.Name);
                    break;

                case BrewerySortBy.City:
                    breweries = query.Descending
                        ? breweries.OrderByDescending(x => x.City)
                        : breweries.OrderBy(x => x.City);
                    break;
            }

            breweries = breweries
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize);

            return await breweries.ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Brewery> breweries)
        {
            await _context.Breweries.AddRangeAsync(breweries);
        }

        public async Task<List<Brewery>> GetForDistanceAsync(string? search)
        {
            IQueryable<Brewery> breweries =
                _context.Breweries.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                breweries = breweries.Where(x =>
                    (x.Name != null &&
                     x.Name.Contains(search)) ||
                    (x.City != null &&
                     x.City.Contains(search)));
            }

            return await breweries.ToListAsync();
        }

        public async Task<List<Brewery>> GetAutocompleteAsync(
            string query,
            int limit)
        {
            return await _context.Breweries
                .AsNoTracking()
                .Where(x =>
                    (x.Name != null &&
                     x.Name.Contains(query)) ||
                    (x.City != null &&
                     x.City.Contains(query)))
                .OrderBy(x => x.Name)
                .Take(limit)
                .ToListAsync();
        }
    }
}
