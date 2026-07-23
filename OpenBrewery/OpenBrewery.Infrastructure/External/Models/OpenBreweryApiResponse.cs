using System.Text.Json.Serialization;

namespace OpenBrewery.Infrastructure.External.Models
{
    public class OpenBreweryApiResponse
    {
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        [JsonPropertyName("brewery_type")]
        public string? BreweryType { get; set; }
        public string? Country { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
    }
}
