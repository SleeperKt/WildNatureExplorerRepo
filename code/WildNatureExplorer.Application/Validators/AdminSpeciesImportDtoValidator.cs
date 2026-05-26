using FluentValidation;
using WildNatureExplorer.Application.DTOs.Admin;

namespace WildNatureExplorer.Application.Validators;

public class AdminSpeciesImportDtoValidator : AbstractValidator<AdminSpeciesImportDto>
{
    public AdminSpeciesImportDtoValidator()
    {
        RuleFor(x => x.CommonName)
            .NotEmpty().MaximumLength(50);

        RuleFor(x => x.ScientificName)
            .NotEmpty().MaximumLength(150);

        RuleFor(x => x.Countries)
            .NotEmpty().WithMessage("At least one country must be specified.")
            .Must(v => SplitAndNormalize(v).Length <= 5)
            .WithMessage("No more than 5 countries allowed.")
            .Must(v => SplitAndNormalize(v).Length >= 1)
            .WithMessage("At least one country must be specified.");

        RuleFor(x => x.Colors)
            .NotEmpty().WithMessage("At least one color must be specified.")
            .Must(v => SplitAndNormalize(v).Length <= 5)
            .WithMessage("No more than 5 colors allowed.")
            .Must(v => SplitAndNormalize(v).Length >= 1)
            .WithMessage("At least one color must be specified.");

        RuleFor(x => x.Habitats)
            .NotEmpty().WithMessage("At least one habitat must be specified.")
            .Must(v => SplitAndNormalize(v).Length <= 5)
            .WithMessage("No more than 5 habitats allowed.")
            .Must(v => SplitAndNormalize(v).Length >= 1)
            .WithMessage("At least one habitat must be specified.");

        RuleFor(x => x.Size)
            .NotEmpty().MaximumLength(50);
    }

    private static string[] SplitAndNormalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<string>();

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
    }
}
