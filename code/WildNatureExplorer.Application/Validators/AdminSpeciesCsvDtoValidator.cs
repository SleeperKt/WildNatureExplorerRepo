using FluentValidation;
using WildNatureExplorer.Application.DTOs.Admin;

namespace WildNatureExplorer.Application.Validators;

public class AdminSpeciesCsvDtoValidator : AbstractValidator<AdminSpeciesCsvDto>
{
    public AdminSpeciesCsvDtoValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull()
            .Must(s => s.CanRead)
            .WithMessage("CSV file stream must be readable.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(name => name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .WithMessage("File must be a .csv file.");
    }
}