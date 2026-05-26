using System.IO;
using System.ComponentModel.DataAnnotations;

namespace WildNatureExplorer.Application.DTOs.Admin
{
    public class AdminSpeciesLocationsCsvDto
    {

        public Guid SpeciesId { get; init; }

        [Required]
        public Stream FileStream { get; init; } = default!;

        [Required]
        public string FileName { get; init; } = string.Empty;
    }
}
