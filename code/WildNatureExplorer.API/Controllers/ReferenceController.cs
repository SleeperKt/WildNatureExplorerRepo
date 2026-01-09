using Microsoft.AspNetCore.Mvc;
using WildNatureExplorer.Application.Interfaces.Repositories;

namespace WildNatureExplorer.API.Controllers;

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

    [HttpGet("countries")]
    public async Task<IActionResult> Countries()
        => Ok(await _countries.GetAllAsync());

    [HttpGet("colors")]
    public async Task<IActionResult> Colors()
        => Ok(await _colors.GetAllAsync());

    [HttpGet("habitats")]
    public async Task<IActionResult> Habitats()
        => Ok(await _habitats.GetAllAsync());

    [HttpGet("sizes")]
    public async Task<IActionResult> Sizes()
        => Ok(await _sizes.GetAllAsync());
}
