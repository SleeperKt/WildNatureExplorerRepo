using System.ComponentModel.DataAnnotations;

namespace WildNatureExplorer.Application.DTOs.Species;

public record SpeciesSearchDto(
    bool? IsDangerous,
    bool? IsRare,

    [MaxLength(5)]
    List<Guid>? CountryIds,

    [MaxLength(5)]
    List<Guid>? HabitatIds,

    [MaxLength(5)]
    List<Guid>? ColorIds,

    [MaxLength(5)]
    List<Guid>? SizeIds
);
