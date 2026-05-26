using FluentValidation;
using WildNatureExplorer.Application.DTOs.Library;

namespace WildNatureExplorer.Application.Validators;

public class CreateSightingRequestValidator : AbstractValidator<CreateSightingRequest>
{
    public CreateSightingRequestValidator()
    {
        // SpeciesId is optional now (free-form saves). When provided, it
        // must resolve to an existing row — checked in the service layer.

        RuleFor(x => x.CommonName)
            .NotEmpty().WithMessage("CommonName is required.")
            .MaximumLength(200);

        RuleFor(x => x.ScientificName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.ScientificName));

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // Accept either a regular http(s) URL or a base64 data: URL of the
        // photo the user uploaded for recognition. Hard cap kept generous
        // (~6 MB worth of base64 text) just to stop runaway payloads.
        RuleFor(x => x.ImageUrl)
            .MaximumLength(8_000_000)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.SightedAt)
            .LessThanOrEqualTo(_ => DateTime.UtcNow.AddMinutes(5))
            .When(x => x.SightedAt.HasValue)
            .WithMessage("SightedAt cannot be in the future.");
    }
}
