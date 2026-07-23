using OpenBrewery.Core.DTOs;
using OpenBrewery.Core.Models;

namespace OpenBrewery.Core.Interfaces
{
    public interface IOpenBreweryService
    {
        public Task<IEnumerable<BreweryDto>> GetBrewery(GetBreweriesRequest getBreweriesRequest);
    }
}