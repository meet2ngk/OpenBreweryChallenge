namespace OpenBrewery.Core.Entities
{
    public class Brewery
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? BreweryType { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
