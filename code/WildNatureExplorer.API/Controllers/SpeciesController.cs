using Microsoft.AspNetCore.Mvc;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.DTOs.Species;

namespace WildNatureExplorer.API.Controllers;

[ApiController]
[Route("api/species")]
public class SpeciesController : ControllerBase
{
    private readonly ISpeciesService _speciesService;

    public SpeciesController(ISpeciesService speciesService)
    {
        _speciesService = speciesService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var species = await _speciesService.GetAsync(id);
        if (species == null)
            return NotFound();

        var response = new SpeciesDetailsDto(
            species.Id,
            species.CommonName,
            species.ScientificName,
            species.Description,
            species.IsDangerous,
            species.IsRare,
            species.Size.Name,
            species.Colors.Select(c => c.Color.Name).ToList(),
            species.Habitats.Select(h => h.Habitat.Name).ToList(),
            species.Countries.Select(c => c.Country.Name).ToList()
        );

        return Ok(response);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SpeciesSearchDto request)
    {
        var result = await _speciesService.SearchAsync(
            request.IsDangerous,
            request.IsRare,
            request.CountryIds,
            request.HabitatIds,
            request.ColorIds,
            request.SizeIds
        );

        var response = result.Select(s => new SpeciesShortDto(
            s.Id,
            s.CommonName,
            s.IsDangerous,
            s.IsRare
        ));

        return Ok(response);
    }

    [HttpGet("by-name")]
    public async Task<IActionResult> GetByCommonName([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Common name is required");

        var species = await _speciesService.GetByCommonNameAsync(name);

        if (species == null)
            return NotFound();

        var response = new SpeciesDetailsDto(
            species.Id,
            species.CommonName,
            species.ScientificName,
            species.Description,
            species.IsDangerous,
            species.IsRare,
            species.Size.Name,
            species.Colors.Select(c => c.Color.Name).ToList(),
            species.Habitats.Select(h => h.Habitat.Name).ToList(),
            species.Countries.Select(c => c.Country.Name).ToList()
        );

        return Ok(response);
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] SpeciesAutocompleteDto request)
    {
        var result = await _speciesService.GetNameSuggestionsAsync(request.Prefix);
        return Ok(result);
    }
}
