using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WildNatureExplorer.Application.Interfaces.Repositories;

namespace WildNatureExplorer.API.Controllers;

/// <summary>
/// Read-only lookup tables for building filter UIs (countries, colours, habitats, animal size classes).
/// </summary>
[ApiController]
[Route("api/reference")]
public class ReferenceController : ControllerBase
{
    private readonly ICountryRepository _countries;
    private readonly IColorRepository _colors;
    private readonly IHabitatRepository _habitats;
    private readonly ISizeRepository _sizes;

    public ReferenceController(
        ICountryRepository countries,
        IColorRepository colors,
        IHabitatRepository habitats,
        ISizeRepository sizes)
    {
        _countries = countries;
        _colors = colors;
        _habitats = habitats;
        _sizes = sizes;
    }

    /// <summary>
    /// All countries available for filtering / mapping flows.
    /// </summary>
    [HttpGet("countries")]
    [SwaggerOperation(Summary = "List all countries.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Countries()
        => Ok(await _countries.GetAllAsync());

    /// <summary>
    /// Colour reference values linked to species metadata.
    /// </summary>
    [HttpGet("colors")]
    [SwaggerOperation(Summary = "List all colour references.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Colors()
        => Ok(await _colors.GetAllAsync());

    /// <summary>
    /// Habitat reference values for species filters.
    /// </summary>
    [HttpGet("habitats")]
    [SwaggerOperation(Summary = "List all habitat references.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Habitats()
        => Ok(await _habitats.GetAllAsync());

    /// <summary>
    /// Size classes (e.g. small/medium/large) for species filters.
    /// </summary>
    [HttpGet("sizes")]
    [SwaggerOperation(Summary = "List all size references.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Sizes()
        => Ok(await _sizes.GetAllAsync());
}
