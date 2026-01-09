using WildNatureExplorer.Application.DTOs.Admin;
using WildNatureExplorer.Application.DTOs.Users;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IAdminImportService
{
    Task ImportSingleSpeciesAsync(AdminSpeciesImportDto dto);
    Task ImportSpeciesCsvAsync(AdminSpeciesCsvDto dto);
    Task ImportSpeciesLocationsCsvAsync(Guid speciesId, Stream fileStream);
    Task ImportSpeciesLocationsFromGbifAsync(Guid speciesId, Guid countryId);
}
