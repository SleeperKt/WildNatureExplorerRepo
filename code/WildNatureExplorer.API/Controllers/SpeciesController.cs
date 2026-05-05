using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WildNatureExplorer.Application.DTOs.Species;
using WildNatureExplorer.Application.Interfaces.Services;

namespace WildNatureExplorer.API.Controllers;

/// <summary>
/// Public species catalogue: lookup, filtered search, name autocomplete.
/// </summary>
[ApiController]
[Route("api/species")]
public class SpeciesController : ControllerBase
{
    private readonly ISpeciesService _speciesService;

    public SpeciesController(ISpeciesService speciesService)
    {
        _speciesService = speciesService;
    }

    /// <summary>
    /// Full species detail including size, colours, habitats, and countries.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get species by unique identifier.")]
    [ProducesResponseType(typeof(SpeciesDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Applies optional danger/rarity filters and up to five GUID lists per dimension (countries, habitats, colours, sizes).
    /// </summary>
    [HttpPost("search")]
    [SwaggerOperation(Summary = "Search species with structured filters.")]
    [ProducesResponseType(typeof(IEnumerable<SpeciesShortDto>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Case-insensitive match on common name; returns 400 when query is empty.
    /// </summary>
    [HttpGet("by-name")]
    [SwaggerOperation(Summary = "Resolve species by common name query string.")]
    [ProducesResponseType(typeof(SpeciesDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Prefix-based suggestions for search boxes (bounded by service implementation).
    /// </summary>
    [HttpGet("autocomplete")]
    [SwaggerOperation(Summary = "Autocomplete common names by prefix.")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Autocomplete([FromQuery] SpeciesAutocompleteDto request)
    {
        var result = await _speciesService.GetNameSuggestionsAsync(request.Prefix);
        return Ok(result);
    }
}
