namespace OpenBrewery.Core.Configuration
{
    public class OpenBreweryApiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string BreweriesEndpoint { get; set; } = string.Empty;
        public int TimeoutInSeconds { get; set; } = 30;
    }
}
