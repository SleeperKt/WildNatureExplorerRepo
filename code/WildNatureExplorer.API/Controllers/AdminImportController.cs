using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WildNatureExplorer.Application.DTOs.Admin;
using WildNatureExplorer.Application.Interfaces.Services;

namespace WildNatureExplorer.API.Controllers;

/// <summary>
/// Bulk and external-data import endpoints for catalogue maintenance (Admin or Moderator).
/// </summary>
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

    /// <summary>
    /// Upserts one species row from validated JSON payload.
    /// </summary>
    [HttpPost("species/single")]
    [SwaggerOperation(Summary = "Import or update a single species from JSON.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportSingle([FromBody] AdminSpeciesImportDto dto)
    {
        await _importService.ImportSingleSpeciesAsync(dto);
        return Ok(new { message = "Species imported successfully." });
    }

    /// <summary>
    /// Parses species rows from an uploaded CSV file (multipart field).
    /// </summary>
    [HttpPost("species/csv")]
    [SwaggerOperation(Summary = "Bulk-import species from CSV file upload.")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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

    /// <summary>
    /// Attaches geographic occurrence rows from CSV to an existing species identifier.
    /// </summary>
    [HttpPost("species/{speciesId:guid}/locations/csv")]
    [SwaggerOperation(Summary = "Import species occurrence points from CSV for one species.")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Pulls occurrence data from GBIF for the given species scoped to one country (external HTTP dependency).
    /// </summary>
    [HttpPost("species/{speciesId:guid}/country/{countryId:guid}/gbif")]
    [SwaggerOperation(Summary = "Import species locations from GBIF API.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
