using OpenBrewery.Core.Entities;

namespace OpenBrewery.Core.Interfaces
{
    public interface IInitializationRepository
    {
        Task<DatabaseInitializationStatus?> GetStatusAsync();
        Task StartAsync();
        Task UpdateProgressAsync(int lastSuccessfulPage);
        Task MarkCompletedAsync();
    }
}