using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenBrewery.Core.Configuration;
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
        private readonly IBreweryRepository _repository;
        private readonly IOptions<WebApiDataSourceOptions> _options;

        public OpenBreweryService(IOpenBreweryClient client, ILogger<OpenBreweryService> logger, IMemoryCache cache, 
                                    IBreweryRepository repository, IOptions<WebApiDataSourceOptions> options)
        {
            _client = client;
            _logger = logger;
            _cache = cache;
            _repository = repository;
            _options = options;
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
                cachedBreweries = await GetBreweriesFromSourceAsync();

                _cache.Set(
                    cacheKey,
                    cachedBreweries,
                    TimeSpan.FromMinutes(10));
            }

            var breweries = cachedBreweries
                .Select(x => new BreweryDto
                {
                    Name = x.Name,
                    City = x.City,
                    Phone = x.Phone,
                    BreweryType = x.BreweryType,
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
                throw new ArgumentException("Both UserLatitude and UserLongitude must be provided.");
            }

            if(sortBy == BrewerySortBy.Distance && !request.UserLatitude.HasValue)
            {
                throw new ArgumentException("User coordinates are required when sorting by distance.");
            }
        }
        private async Task<IList<BreweryDto>> GetBreweriesFromSourceAsync()
        {
            _logger.LogInformation("Configured brewery data source: {DataSource}", _options.Value.DataSource);

            if (_options.Value.DataSource == BreweryDataSource.Database)
            {
                return await GetBreweriesFromDatabaseAsync();
            }

            return await GetBreweriesFromExternalApiAsync();
        }

        private async Task<IList<BreweryDto>> GetBreweriesFromExternalApiAsync()
        {
            _logger.LogInformation("Fetching breweries from external API.");

            var apiResponse = await _client.GetBreweriesAsync();

            return apiResponse.Select(x => new BreweryDto
            {
                Name = x.Name,
                City = x.City,
                Phone = x.Phone,
                BreweryType = x.BreweryType,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                DistanceInKm = null
            }).ToList();
        }

        private async Task<IList<BreweryDto>> GetBreweriesFromDatabaseAsync()
        {
            var breweriesFromDatabase = await _repository.GetAllAsync();

            if (breweriesFromDatabase.Any())
            {
                _logger.LogInformation("Returning {Count} breweries from SQLite.", breweriesFromDatabase.Count);

                return breweriesFromDatabase
                    .Select(x => new BreweryDto
                    {
                        Name = x.Name,
                        City = x.City,
                        Phone = x.Phone,
                        BreweryType = x.BreweryType,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,
                        DistanceInKm = null
                    })
                    .ToList();
            }

            _logger.LogInformation("SQLite database is empty. Fetching breweries from external API for initial population.");

            var breweriesFromApi = await _client.GetBreweriesAsync();

            var breweryEntities = breweriesFromApi
                .Select(x => new Core.Entities.Brewery
                {
                    Name = x.Name,
                    City = x.City,
                    Phone = x.Phone,
                    BreweryType = x.BreweryType,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude
                })
                .ToList();

            await _repository.SeedAsync(breweryEntities);
            _logger.LogInformation("Seeded {Count} breweries into SQLite.", breweryEntities.Count);

            return breweriesFromApi
                .Select(x => new BreweryDto
                {
                    Name = x.Name,
                    City = x.City,
                    Phone = x.Phone,
                    BreweryType = x.BreweryType,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    DistanceInKm = null
                })
                .ToList();
        }
    }
}