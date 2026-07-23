using System.Text.Json.Serialization;

namespace OpenBrewery.Core.DTOs
{
    public class BreweryDto
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string BrowserType { get; set; }
        public double? DistanceInKm { get; set; }
        [JsonIgnore]
        public double? Latitude { get; set; }
        [JsonIgnore]
        public double? Longitude { get; set; }
    }
}
