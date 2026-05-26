using FluentValidation;
using WildNatureExplorer.Application.DTOs.Library;

namespace WildNatureExplorer.Application.Validators;

public class NearbySightingsRequestValidator : AbstractValidator<NearbySightingsRequest>
{
    public NearbySightingsRequestValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");

        RuleFor(x => x.RadiusKm)
            .GreaterThan(0).WithMessage("RadiusKm must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("RadiusKm cannot exceed 1000 km.");
    }
}
