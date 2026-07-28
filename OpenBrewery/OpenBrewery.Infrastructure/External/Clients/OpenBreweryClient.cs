using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenBrewery.Core.Configuration;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Infrastructure.External.Models;
using System.Net.Http.Json;

namespace OpenBrewery.Infrastructure.External.Clients
{
    public class OpenBreweryClient : IOpenBreweryClient
    {

        private readonly ILogger<OpenBreweryClient> _logger;
        private readonly HttpClient _httpClient;
        private readonly IOptions<OpenBreweryApiOptions> _options;

        public OpenBreweryClient(ILogger<OpenBreweryClient> logger, HttpClient client, IOptions<OpenBreweryApiOptions> options)
        {
            _logger = logger;
            _httpClient = client;
            _options = options;
            _httpClient.BaseAddress = new Uri(_options.Value.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.Value.TimeoutInSeconds);
        }
        public async Task<IEnumerable<OpenBreweryApiResponse>> GetBreweriesAsync(
            int page,
            int perPage,
            string? search = null,
             string? searchBy = null,
            string? sortBy = null,
            bool descending = false,
            double? latitude = null,
            double? longitude = null)
        {
            try
            {
                var query = new Dictionary<string, string?>
                {
                    ["page"] = page.ToString(),
                    ["per_page"] = perPage.ToString()
                };

                if (!string.IsNullOrWhiteSpace(search) &&
                        !string.IsNullOrWhiteSpace(searchBy))
                {
                    switch (searchBy.ToLowerInvariant())
                    {
                        case "name":
                            query["by_name"] = search;
                            break;

                        case "city":
                            query["by_city"] = search;
                            break;

                        case "distance":
                            if (latitude.HasValue && longitude.HasValue)
                            {
                                query["by_dist"] =
                                    $"{latitude.Value},{longitude.Value}";
                            }
                            break;
                    }
                }

                var requestUri = QueryHelpers.AddQueryString(
                                    _options.Value.BreweriesEndpoint,
                                    query);

                if (string.Equals(
                    sortBy,
                    "distance",
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (latitude.HasValue &&
                        longitude.HasValue)
                    {
                        requestUri =
                            $"{requestUri}&by_dist={latitude.Value},{longitude.Value}";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    var upstreamSort = MapSortField(sortBy);

                    if (upstreamSort is not null)
                    {
                        var sortExpression =
                            descending
                                ? $"{upstreamSort}.desc"
                                : $"{upstreamSort}.asc";

                        requestUri =
                            $"{requestUri}&{sortExpression}";
                    }
                }

                _logger.LogInformation(
                    "Calling Open Brewery external API: {RequestUri}",
                    requestUri);

                var response = await _httpClient.GetAsync(requestUri);

                _logger.LogInformation(
                    "Open Brewery external API responded with status code {StatusCode}",
                    response.StatusCode);

                response.EnsureSuccessStatusCode();

                var breweries =
                    await response.Content.ReadFromJsonAsync<
                        List<OpenBreweryApiResponse>>()
                    ?? new List<OpenBreweryApiResponse>();

                _logger.LogInformation(
                    "Retrieved {Count} breweries from Open Brewery external API",
                    breweries.Count);

                return breweries;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Error while calling Open Brewery API");

                throw;
            }
        }

        public async Task<IEnumerable<OpenBreweryApiResponse>> SearchBreweriesAsync(
            string query,
            int perPage)
        {
            try
            {
                var queryParameters = new Dictionary<string, string?>
                {
                    ["query"] = query,
                    ["per_page"] = perPage.ToString()
                };

                var requestUri = QueryHelpers.AddQueryString(
                    $"{_options.Value.BreweriesEndpoint}/search",
                    queryParameters);

                _logger.LogInformation(
                    "Calling Open Brewery search API: {RequestUri}",
                    requestUri);

                var response = await _httpClient.GetAsync(requestUri);

                _logger.LogInformation(
                    "Open Brewery search API responded with status code {StatusCode}",
                    response.StatusCode);

                response.EnsureSuccessStatusCode();

                var breweries =
                    await response.Content.ReadFromJsonAsync<
                        List<OpenBreweryApiResponse>>()
                    ?? new List<OpenBreweryApiResponse>();

                _logger.LogInformation(
                    "Retrieved {Count} breweries from Open Brewery search API",
                    breweries.Count);

                return breweries;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Error while calling Open Brewery search API");

                throw;
            }
        }

        private static string? MapSortField(string? sortBy)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "name" => "name",
                "city" => "city",
                _ => null
            };
        }
    }
}
