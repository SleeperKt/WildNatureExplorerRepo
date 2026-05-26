using FluentValidation;
using WildNatureExplorer.Application.DTOs.AI;

namespace WildNatureExplorer.Application.Validators
{
    public class AiFeedbackDtoValidator : AbstractValidator<AiFeedbackDto>
    {
        public AiFeedbackDtoValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 100)
                .WithMessage("Rating must be between 1 and 100.");

            RuleFor(x => x.Comment)
                .MaximumLength(100)
                .WithMessage("Comment cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Comment));
        }
    }
}
