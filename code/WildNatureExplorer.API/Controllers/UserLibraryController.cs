using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Application.DTOs.Library;
using WildNatureExplorer.Application.Interfaces.Services;

namespace WildNatureExplorer.API.Controllers;

/// <summary>
/// User Library — every authenticated user has their own private list of
/// "animals I've met", with map coordinates and an optional photo.
/// </summary>
[ApiController]
[Route("api/library")]
[Authorize]
public class UserLibraryController : ControllerBase
{
    private readonly IUserLibraryService _library;

    /// <summary>Creates a new <see cref="UserLibraryController"/>.</summary>
    public UserLibraryController(IUserLibraryService library)
    {
        _library = library;
    }

    /// <summary>
    /// Save a recognised animal to the current user's library.
    /// </summary>
    [HttpPost("sightings")]
    [ProducesResponseType(typeof(SightingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSightingRequest request)
    {
        var userId = CurrentUserId();
        var dto = await _library.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>
    /// Get all of the current user's saved sightings (used by the library map).
    /// </summary>
    [HttpGet("sightings")]
    [ProducesResponseType(typeof(IEnumerable<SightingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine()
    {
        var userId = CurrentUserId();
        var rows = await _library.GetMyLibraryAsync(userId);
        return Ok(rows);
    }

    /// <summary>
    /// Get a single sighting (must belong to the caller).
    /// </summary>
    [HttpGet("sightings/{id:guid}")]
    [ProducesResponseType(typeof(SightingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = CurrentUserId();
        var dto = await _library.GetByIdAsync(userId, id);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    /// <summary>
    /// Remove a saved sighting.
    /// </summary>
    [HttpDelete("sightings/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = CurrentUserId();
        try
        {
            await _library.DeleteAsync(userId, id);
            return NoContent();
        }
        catch (ResourceNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Find sightings owned by the current user inside a circle around (lat,lng).
    /// Powered by the PostgreSQL function <c>fn_user_nearby_sightings</c>.
    /// </summary>
    /// <param name="lat">Latitude of the search origin (degrees).</param>
    /// <param name="lng">Longitude of the search origin (degrees).</param>
    /// <param name="radiusKm">Search radius in kilometres (0, 1000].</param>
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(IEnumerable<NearbySightingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radiusKm = 25)
    {
        var userId = CurrentUserId();
        var request = new NearbySightingsRequest
        {
            Latitude = lat,
            Longitude = lng,
            RadiusKm = radiusKm
        };
        var rows = await _library.GetNearbyAsync(userId, request);
        return Ok(rows);
    }

    private Guid CurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
