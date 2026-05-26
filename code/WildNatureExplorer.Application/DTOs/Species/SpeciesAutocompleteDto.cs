using System.ComponentModel.DataAnnotations;

namespace WildNatureExplorer.Application.DTOs.Species;

public record SpeciesAutocompleteDto(
    [Required]
    [MaxLength(50)]
    string Prefix
);
