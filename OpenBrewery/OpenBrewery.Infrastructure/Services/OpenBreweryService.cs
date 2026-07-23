using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenBrewery.Core.DTOs;
using OpenBrewery.Core.Enums;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Core.Models;
using OpenBrewery.Core.Utilities;


namespace OpenBrewery.Infrastructure.Services
{
    public class OpenBreweryService : IOpenBreweryService
    {
        private readonly IOpenBreweryClient _client;
        private readonly ILogger<OpenBreweryService> _logger;
        private readonly IMemoryCache _cache;

        public OpenBreweryService(IOpenBreweryClient client, ILogger<OpenBreweryService> logger, IMemoryCache cache)
        {
            _client = client;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IEnumerable<BreweryDto>> GetBreweryAsync(GetBreweriesRequest request)
        {
            const string cacheKey = "Breweries";
            IList<BreweryDto> cachedBreweries;
            Validate(request);

            if (_cache.TryGetValue(cacheKey, out IList<BreweryDto>? breweriesFromCache))
            {
                _logger.LogInformation("Returning breweries from cache");

                cachedBreweries = breweriesFromCache;
            }
            else
            {
                var apiResponse = await _client.GetBreweriesAsync();

                cachedBreweries = apiResponse.Select(x => new BreweryDto
                {
                    Name = x.Name,
                    City = x.City,
                    Phone = x.Phone,
                    BrowserType = x.BreweryType,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    DistanceInKm = null
                }).ToList();

                _cache.Set(cacheKey, cachedBreweries, TimeSpan.FromMinutes(10));
            }

            var breweries = cachedBreweries
                .Select(x => new BreweryDto
                {
                    Name = x.Name,
                    City = x.City,
                    Phone = x.Phone,
                    BrowserType = x.BrowserType,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                breweries = breweries.Where(b =>
                    (!string.IsNullOrWhiteSpace(b.Name) && b.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                    !string.IsNullOrWhiteSpace(b.City) && b.City.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                    ).ToList();

                _logger.LogInformation("Search '{Search}' filtered breweries to {FilteredCount}.", request.Search, breweries.Count);
            }

            BrewerySortBy? sortBy = null;
            if (Enum.TryParse<BrewerySortBy>(request.SortBy, true, out var parsedSortBy))
            {
                sortBy = parsedSortBy;
            }

            if(sortBy.HasValue) 
            {
                switch(sortBy.Value)
                {
                    case BrewerySortBy.Name:
                        breweries = request.Descending
                            ? breweries.OrderByDescending(x => x.Name).ToList()
                            : breweries.OrderBy(x => x.Name).ToList();
                        break;
                    case BrewerySortBy.City:
                        breweries = request.Descending
                            ? breweries.OrderByDescending(x => x.City).ToList()
                            : breweries.OrderBy(x => x.City).ToList();
                        break;
                    case BrewerySortBy.Distance:

                        foreach(var brewery in breweries.Where(x => x.Latitude.HasValue && x.Longitude.HasValue))
                        {
                            var distance = GeoDistanceCalculator.GeoDistanceCalculate(request.UserLatitude.Value, request.UserLongitude.Value, 
                                                                            brewery.Latitude.Value, brewery.Longitude.Value);
                            brewery.DistanceInKm = distance;
                        }

                        breweries = breweries.Where(x=> x.DistanceInKm.HasValue).ToList();

                        breweries = request.Descending
                            ? breweries.OrderByDescending(x => x.DistanceInKm).ToList()
                            : breweries.OrderBy(x => x.DistanceInKm).ToList();
                        break;
                }

                _logger.LogInformation("Sorted breweries by '{SortBy}' in order '{Order}'.", request.SortBy, request.Descending ? "Descending" : "Ascending");
            }          

            return breweries;
        }
    
        private static void Validate(GetBreweriesRequest request)
        {
            BrewerySortBy? sortBy = null;

            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                if (!Enum.TryParse<BrewerySortBy>(request.SortBy, ignoreCase: true, out var parsedSortBy))
                {
                    throw new ArgumentException($"Invalid sortBy value '{request.SortBy}'");
                }
                sortBy = parsedSortBy;
            }

            if (request.UserLatitude.HasValue != request.UserLongitude.HasValue)
            {
                throw new ArgumentException("Both UserLatitude and UserLongiture must be provided.");
            }

            if(sortBy == BrewerySortBy.Distance && !request.UserLatitude.HasValue)
            {
                throw new ArgumentException("User coordinates are required when sorting by distance.");
            }
        }
    }
}