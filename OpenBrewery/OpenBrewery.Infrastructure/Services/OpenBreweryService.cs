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
        private readonly CacheOptions _cacheOptions;

        public OpenBreweryService(
            IOpenBreweryClient client,
            ILogger<OpenBreweryService> logger,
            IMemoryCache cache,
            IBreweryRepository repository,
            IOptions<WebApiDataSourceOptions> options,
            IOptions<CacheOptions> cacheOptions)
        {
            _client = client;
            _logger = logger;
            _cache = cache;
            _repository = repository;
            _options = options;
            _cacheOptions = cacheOptions.Value;
        }

        public async Task<IEnumerable<BreweryDto>> GetBreweryAsync(
            GetBreweriesRequest request)
        {
            Validate(request);

            var cacheKey =
                $"Breweries:" +
                $"page={request.PageNumber}:" +
                $"size={request.PageSize}:" +
                $"search={request.Search}:" +
                $"searchBy={request.SearchBy}:" +
                $"sort={request.SortBy}:" +
                $"desc={request.Descending}:" +
                $"lat={request.UserLatitude}:" +
                $"lon={request.UserLongitude}";

            if (_cache.TryGetValue(
                cacheKey,
                out IList<BreweryDto>? breweriesFromCache))
            {
                _logger.LogInformation(
                    "Returning breweries from cache. Page: {PageNumber}, SearchBy: {SearchBy}",
                    request.PageNumber,
                    request.SearchBy);

                return breweriesFromCache;
            }

            var breweries =
                await GetBreweriesFromSourceAsync(request);

            _cache.Set(
                cacheKey,
                breweries,
                TimeSpan.FromMinutes(
                    _cacheOptions.ExpirationInMinutes));

            return breweries;
        }

        private static void Validate(
            GetBreweriesRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchBy))
            {
                var validSearchBy =
                    new[] { "name", "city" };

                if (!validSearchBy.Contains(
                    request.SearchBy,
                    StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        $"Invalid SearchBy value '{request.SearchBy}'. " +
                        "Valid values are name or city.");
                }
            }

            BrewerySortBy? sortBy = null;

            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                if (!Enum.TryParse<BrewerySortBy>(
                    request.SortBy,
                    ignoreCase: true,
                    out var parsedSortBy))
                {
                    throw new ArgumentException(
                        $"Invalid sortBy value '{request.SortBy}'");
                }

                sortBy = parsedSortBy;
            }

            if (request.UserLatitude.HasValue !=
                request.UserLongitude.HasValue)
            {
                throw new ArgumentException(
                    "Both UserLatitude and UserLongitude must be provided.");
            }

            if ((string.Equals(
                    request.SearchBy,
                    "distance",
                    StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(
                    request.SortBy,
                    "distance",
                    StringComparison.OrdinalIgnoreCase)) &&
                (!request.UserLatitude.HasValue ||
                 !request.UserLongitude.HasValue))
            {
                throw new ArgumentException(
                    "User coordinates are required when searching or sorting by distance.");
            }

            if (request.PageNumber < 1)
            {
                throw new ArgumentException(
                    "PageNumber must be greater than 0.");
            }

            if (request.PageSize < 1 ||
                request.PageSize > 200)
            {
                throw new ArgumentException(
                    "PageSize must be between 1 and 200.");
            }
        }

        private async Task<IList<BreweryDto>>
            GetBreweriesFromSourceAsync(
                GetBreweriesRequest request)
        {
            _logger.LogInformation(
                "Configured brewery data source: {DataSource}",
                _options.Value.DataSource);

            if (_options.Value.DataSource ==
                BreweryDataSource.Database)
            {
                return await GetBreweriesFromDatabaseAsync(
                    request);
            }

            return await GetBreweriesFromExternalApiAsync(
                request);
        }

        private async Task<IList<BreweryDto>>
            GetBreweriesFromExternalApiAsync(
                GetBreweriesRequest request)
        {
            _logger.LogInformation(
                "Fetching breweries from external API. " +
                "Page: {Page}, PageSize: {PageSize}, SearchBy: {SearchBy}",
                request.PageNumber,
                request.PageSize,
                request.SearchBy);

            var apiResponse =
                await _client.GetBreweriesAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.Search,
                    request.SearchBy,
                    request.SortBy,
                    request.Descending,
                    request.UserLatitude,
                    request.UserLongitude);

            return apiResponse
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
        }

        private async Task<IList<BreweryDto>>
            GetBreweriesFromDatabaseAsync(
                GetBreweriesRequest request)
        {
            var sortBy =
                Enum.TryParse<BrewerySortBy>(
                    request.SortBy,
                    true,
                    out var parsedSortBy)
                    ? parsedSortBy
                    : (BrewerySortBy?)null;

            if (string.Equals(
                    request.SortBy,
                    "distance",
                    StringComparison.OrdinalIgnoreCase))
            {
                return await
                    GetBreweriesFromDatabaseByDistanceAsync(
                        request);
            }

            var query = new BreweryQuery
            {
                Search = request.Search,
                SearchBy = request.SearchBy,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortBy = sortBy,
                Descending = request.Descending
            };

            var breweriesFromDatabase =
                await _repository.GetAllAsync(query);

            _logger.LogInformation(
                "Retrieved {Count} breweries from SQLite.",
                breweriesFromDatabase.Count);

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

        private async Task<IList<BreweryDto>>
            GetBreweriesFromDatabaseByDistanceAsync(
                GetBreweriesRequest request)
        {
            var breweriesFromDatabase =
                await _repository.GetForDistanceAsync(
                    request.Search);

            var breweries = breweriesFromDatabase
                .Where(x =>
                    x.Latitude.HasValue &&
                    x.Longitude.HasValue)
                .Select(x =>
                {
                    var distance =
                        GeoDistanceCalculator
                            .GeoDistanceCalculate(
                                request.UserLatitude!.Value,
                                request.UserLongitude!.Value,
                                x.Latitude!.Value,
                                x.Longitude!.Value);

                    return new BreweryDto
                    {
                        Name = x.Name,
                        City = x.City,
                        Phone = x.Phone,
                        BreweryType = x.BreweryType,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,
                        DistanceInKm = distance
                    };
                });

            breweries = request.Descending
                ? breweries.OrderByDescending(
                    x => x.DistanceInKm)
                : breweries.OrderBy(
                    x => x.DistanceInKm);

            return breweries
                .Skip(
                    (request.PageNumber - 1) *
                    request.PageSize)
                .Take(request.PageSize)
                .ToList();
        }
    }
}