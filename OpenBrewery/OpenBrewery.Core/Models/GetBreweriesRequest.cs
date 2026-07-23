namespace OpenBrewery.Core.Models
{
    public class GetBreweriesRequest
    {
        public string? Search {  get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
        public double? UserLongitude { get; set; }
        public double? UserLatitude { get; set; }
    }
}
