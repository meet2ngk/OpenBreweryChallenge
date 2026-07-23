using Microsoft.Extensions.Logging;
using OpenBrewery.Core.DTOs;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Core.Models;


namespace OpenBrewery.Infrastructure.Services
{
    public class OpenBreweryService : IOpenBreweryService
    {
        private readonly IOpenBreweryClient _client;
        private readonly ILogger<OpenBreweryService> _logger;

        public OpenBreweryService(IOpenBreweryClient client, ILogger<OpenBreweryService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<IEnumerable<BreweryDto>> GetBrewery(GetBreweriesRequest getBreweriesRequest)
        {
            var apiResponse = await _client.GetBreweriesAsync();

            var breweries = apiResponse.Select(x => new BreweryDto
                                                {
                                                    Name = x.Name,
                                                    City = x.City,
                                                    Phone = x.Phone
                                                })
                                                .ToList();
            return breweries;
        }
    }
}
