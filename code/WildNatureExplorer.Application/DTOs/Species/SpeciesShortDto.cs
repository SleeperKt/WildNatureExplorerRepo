namespace WildNatureExplorer.Application.DTOs.Species;

public record SpeciesShortDto(
    Guid Id,
    string CommonName,
    bool IsDangerous,
    bool IsRare
);
