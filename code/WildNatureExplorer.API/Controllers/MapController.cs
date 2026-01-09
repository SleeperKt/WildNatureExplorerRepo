using Microsoft.AspNetCore.Mvc;
using WildNatureExplorer.Application.Interfaces.Repositories;

namespace WildNatureExplorer.API.Controllers;

[ApiController]
[Route("api/map")]
public class MapController : ControllerBase
{
    private readonly ISpeciesRepository _speciesRepository;

    public MapController(ISpeciesRepository speciesRepository)
    {
        _speciesRepository = speciesRepository;
    }

    [HttpGet("country/{countryId:guid}")]
    public async Task<IActionResult> GetByCountry(Guid countryId)
    {
        var species = await _speciesRepository.GetByCountryAsync(countryId);

        var response = species.SelectMany(s => s.Locations.Select(l => new
        {
            SpeciesId = s.Id,
            s.CommonName,
            l.Latitude,
            l.Longitude
        }));

        return Ok(response);
    }
}
