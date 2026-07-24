using System.ComponentModel.DataAnnotations;

namespace OpenBrewery.Core.Models
{
    public class GetBreweriesRequest
    {
        [MaxLength(100, ErrorMessage = "Search cannot exceed 100 characters.")]
        public string? Search {  get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
        public double? UserLongitude { get; set; }
        public double? UserLatitude { get; set; }
    }
}
