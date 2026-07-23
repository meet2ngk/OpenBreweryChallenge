using OpenBrewery.Infrastructure.External.Models;

namespace OpenBrewery.Core.Interfaces
{
    public interface IOpenBreweryClient
    {
        public Task<IEnumerable<OpenBreweryApiResponse>> GetBreweriesAsync();
    }
}
