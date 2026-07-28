using OpenBrewery.Infrastructure.External.Models;

namespace OpenBrewery.Core.Interfaces
{
    public interface IOpenBreweryClient
    {
        Task<IEnumerable<OpenBreweryApiResponse>> GetBreweriesAsync(
                                                                    int page,
                                                                    int perPage,
                                                                    string? search = null,
                                                                    string? searchBy = null,
                                                                    string? sortBy = null,
                                                                    bool descending = false,
                                                                    double? latitude = null,
                                                                    double? longitude = null);

        Task<IEnumerable<OpenBreweryApiResponse>> SearchBreweriesAsync(
                                                                    string query,
                                                                    int perPage);
    }
}
