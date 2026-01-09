using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WildNatureExplorer.Application.DTOs.Admin;
using WildNatureExplorer.Application.Interfaces.Services;

namespace WildNatureExplorer.API.Controllers;

[ApiController]
[Route("api/admin/import")]
[Authorize(Roles = "Admin,Moderator")]
public class AdminImportController : ControllerBase
{
    private readonly IAdminImportService _importService;

    public AdminImportController(IAdminImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("species/single")]
    public async Task<IActionResult> ImportSingle([FromBody] AdminSpeciesImportDto dto)
    {
        await _importService.ImportSingleSpeciesAsync(dto);
        return Ok(new { message = "Species imported successfully." });
    }

    [HttpPost("species/csv")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        var dto = new AdminSpeciesCsvDto
        {
            FileStream = file.OpenReadStream(),
            FileName = file.FileName
        };

        await _importService.ImportSpeciesCsvAsync(dto);
        return Ok(new { message = "CSV imported successfully." });
    }

    [HttpPost("species/{speciesId:guid}/locations/csv")]
    public async Task<IActionResult> ImportSpeciesLocations([FromRoute] Guid speciesId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        var dto = new AdminSpeciesLocationsCsvDto
        {
            SpeciesId = speciesId,
            FileStream = file.OpenReadStream(),
            FileName = file.FileName
        };

        await _importService.ImportSpeciesLocationsCsvAsync(dto.SpeciesId, dto.FileStream);
        return Ok(new { message = "Species locations imported successfully." });
    }

    [HttpPost("species/{speciesId:guid}/country/{countryId:guid}/gbif")]
    public async Task<IActionResult> ImportFromGbif([FromRoute] Guid speciesId, [FromRoute] Guid countryId)
    {
        try
        {
            await _importService.ImportSpeciesLocationsFromGbifAsync(speciesId, countryId);
            return Ok(new { message = "Species locations imported from GBIF successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error importing from GBIF: {ex.Message}" });
        }
    }
}
