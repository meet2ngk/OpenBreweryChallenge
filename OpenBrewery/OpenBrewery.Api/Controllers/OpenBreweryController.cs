using Microsoft.AspNetCore.Mvc;
using OpenBrewery.Core.DTOs;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Core.Models;

namespace OpenBrewery.Api.Controllers
{
    [ApiController]
    [Route("api/v1/breweries")]
    public class OpenBreweryController : ControllerBase
    {

        private readonly ILogger<OpenBreweryController> _logger;
        private readonly IOpenBreweryService _openBreweryService;

        public OpenBreweryController(ILogger<OpenBreweryController> logger, IOpenBreweryService openBreweryService)
        {
            _logger = logger;
            _openBreweryService = openBreweryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BreweryDto>>> Get([FromQuery]GetBreweriesRequest getBreweriesRequest)
        {
            try
            {

                _logger.LogInformation("Fetching breweries");

                var breweries = await _openBreweryService.GetBreweryAsync(getBreweriesRequest);

                if(!breweries.Any())
                {
                    _logger.LogInformation("No breweries found for '{Search}'", getBreweriesRequest?.Search);
                    return NotFound("No breweries found.");
                }

                _logger.LogInformation("Retreived {Count} breweries.", breweries.Count());

                return Ok(breweries);

            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                return BadRequest(ex.Message);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error occured while retreiving breweries.");

                return Problem(title: "An unexpected error occured",
                               statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}