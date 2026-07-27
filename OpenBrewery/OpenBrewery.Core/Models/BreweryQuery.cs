using OpenBrewery.Core.Enums;

namespace OpenBrewery.Core.Models
{
    public class BreweryQuery
    {
        public string? Search { get; set; }
        public string? SearchBy { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public BrewerySortBy? SortBy { get; set; }
        public bool Descending { get; set; }
        public double? UserLatitude { get; set; }
        public double? UserLongitude { get; set; }
    }
}