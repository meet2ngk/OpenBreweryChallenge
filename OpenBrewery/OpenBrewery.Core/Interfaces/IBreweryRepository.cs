using OpenBrewery.Core.Entities;
using OpenBrewery.Core.Models;

namespace OpenBrewery.Core.Interfaces
{
    public interface IBreweryRepository
    {
        Task<List<Brewery>> GetAllAsync(BreweryQuery query);
        Task AddRangeAsync(IEnumerable<Brewery> breweries);
        Task<List<Brewery>> GetForDistanceAsync(string? search);
    }
}