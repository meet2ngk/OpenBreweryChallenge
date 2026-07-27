using Microsoft.Extensions.Logging;
using OpenBrewery.Core.Entities;
using OpenBrewery.Core.Interfaces;

namespace OpenBrewery.Infrastructure.Services
{
    public class OpenBreweryDataSyncService : IBreweryDataSyncService
    {
        private readonly IOpenBreweryClient _client;
        private readonly IBreweryRepository _breweryRepository;
        private readonly IInitializationRepository _initializationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OpenBreweryDataSyncService> _logger;

        public OpenBreweryDataSyncService(
            IOpenBreweryClient client,
            IBreweryRepository breweryRepository,
            IInitializationRepository initializationRepository,
            IUnitOfWork unitOfWork,
            ILogger<OpenBreweryDataSyncService> logger)
        {
            _client = client;
            _breweryRepository = breweryRepository;
            _initializationRepository = initializationRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task InitializeDatabaseAsync()
        {
            const int pageSize = 200;

            try
            {
                var status = await _initializationRepository.GetStatusAsync();

                if (status?.IsCompleted == true)
                {
                    _logger.LogInformation(
                        "Brewery database initialization has already completed.");

                    return;
                }

                await _initializationRepository.StartAsync();
                await _unitOfWork.SaveChangesAsync();

                var startPage = (status?.LastSuccessfulPage ?? 0) + 1;
                var pageNumber = startPage;

                while (true)
                {
                    _logger.LogInformation(
                        "Fetching brewery data. Page: {PageNumber}, PageSize: {PageSize}",
                        pageNumber,
                        pageSize);

                    var breweries = await _client.GetBreweriesAsync(
                        pageNumber,
                        pageSize);

                    var breweryList = breweries.ToList();

                    if (breweryList.Count == 0)
                    {
                        break;
                    }

                    var entities = breweryList
                        .Select(x => new Brewery
                        {
                            Name = x.Name,
                            City = x.City,
                            Phone = x.Phone,
                            BreweryType = x.BreweryType,
                            Latitude = x.Latitude,
                            Longitude = x.Longitude
                        })
                        .ToList();

                    await _unitOfWork.BeginTransactionAsync();

                    try
                    {
                        await _breweryRepository.AddRangeAsync(entities);
                        await _initializationRepository.UpdateProgressAsync(pageNumber);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        _logger.LogInformation(
                            "Successfully processed page {PageNumber}.",
                            pageNumber);
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        _logger.LogError(
                            ex,
                            "Failed to process page {PageNumber}. Transaction rolled back.",
                            pageNumber);

                        throw;
                    }

                    if (breweryList.Count < pageSize)
                    {
                        break;
                    }

                    pageNumber++;
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _initializationRepository.MarkCompletedAsync();
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation(
                        "Brewery database initialization completed successfully.");
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    _logger.LogError(
                        ex,
                        "Failed to mark brewery database initialization as completed.");

                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Brewery database initialization failed.");

                throw;
            }
        }
    }
}