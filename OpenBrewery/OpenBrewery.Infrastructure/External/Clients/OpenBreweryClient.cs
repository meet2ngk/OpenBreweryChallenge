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
        public async Task<IEnumerable<OpenBreweryApiResponse>> GetBreweriesAsync()
        {
            try
            {
                _logger.LogInformation("Calling Open Brewery external API");

                var response = await _httpClient.GetAsync(_options.Value.BreweriesEndpoint);

                _logger.LogInformation("Open Brewery external API responsed with status code {StatusCode}",response.StatusCode);

                response.EnsureSuccessStatusCode();

                var breweries = await response.Content.ReadFromJsonAsync<List<OpenBreweryApiResponse>>();

                _logger.LogInformation("Retreived {Count} breweries from Open Brewery external API", breweries.Count);

                return breweries ?? Enumerable.Empty<OpenBreweryApiResponse>();

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error while calling Open Brewery API");
                throw;
            }
        }
    }
}
