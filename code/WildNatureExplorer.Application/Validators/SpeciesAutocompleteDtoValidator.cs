using FluentValidation;
using WildNatureExplorer.Application.DTOs.Species;

namespace WildNatureExplorer.Application.Validators;

public class SpeciesAutocompleteDtoValidator : AbstractValidator<SpeciesAutocompleteDto>
{
    public SpeciesAutocompleteDtoValidator()
    {
        RuleFor(x => x.Prefix)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(50);
    }
}
