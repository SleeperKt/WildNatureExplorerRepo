using System.ComponentModel.DataAnnotations;

namespace WildNatureExplorer.Application.DTOs.Admin;

public record AdminSpeciesImportDto(
    [Required]
    string CommonName,

    [Required]
    string ScientificName,

    string? Description,

    bool IsDangerous,

    bool IsRare,

    [Required]
    string Countries,

    [Required]
    string Colors,

    [Required]
    string Habitats,

    [Required]
    string Size
);
