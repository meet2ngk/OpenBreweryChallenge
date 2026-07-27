namespace OpenBrewery.Core.Entities
{
    public class DatabaseInitializationStatus
    {
        public int Id { get; set; }
        public bool IsCompleted { get; set; }
        public int LastSuccessfulPage { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
