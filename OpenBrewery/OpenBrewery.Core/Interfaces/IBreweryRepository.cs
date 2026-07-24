using OpenBrewery.Core.Entities;

namespace OpenBrewery.Core.Interfaces
{
    public interface IBreweryRepository
    {
        public Task<List<Brewery>> GetAllAsync();
        public Task SeedAsync(IEnumerable<Brewery> breweries);
    }
}
