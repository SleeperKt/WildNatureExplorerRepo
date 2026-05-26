namespace WildNatureExplorer.Application.DTOs.Species;

public record SpeciesDetailsDto(
    Guid Id,
    string CommonName,
    string ScientificName,
    string Description,
    bool IsDangerous,
    bool IsRare,
    string Size,
    List<string> Colors,
    List<string> Habitats,
    List<string> Countries
);
