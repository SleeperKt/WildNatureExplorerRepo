using FluentValidation;
using WildNatureExplorer.Application.DTOs.AI;

namespace WildNatureExplorer.Application.Validators
{
    public class AiQuestionDtoValidator : AbstractValidator<AiQuestionDto>
    {
        public AiQuestionDtoValidator()
        {
            RuleFor(x => x.QuestionAboutNature)
                .NotEmpty().WithMessage("Question is required.")
                .MinimumLength(8).WithMessage("Question must be at least 8 characters.")
                .MaximumLength(100).WithMessage("Question cannot exceed 100 characters.")
                .Must(q => q.Trim().EndsWith("?"))
                .WithMessage("Question must end with a '?'.");
        }
    }
}
