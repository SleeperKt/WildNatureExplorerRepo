using FluentValidation;
using WildNatureExplorer.Application.DTOs.Species;

namespace WildNatureExplorer.Application.Validators;

public class SpeciesSearchDtoValidator : AbstractValidator<SpeciesSearchDto>
{
    public SpeciesSearchDtoValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                (x.CountryIds?.Any() ?? false) ||
                (x.HabitatIds?.Any() ?? false) ||
                (x.ColorIds?.Any() ?? false) ||
                (x.SizeIds?.Any() ?? false))
            .WithMessage("At least one filter must be selected.");
    }
}
