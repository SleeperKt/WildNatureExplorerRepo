using FluentValidation;
using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.Interfaces.Services;

namespace WildNatureExplorer.Application.Validators
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator(IUserService userService)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("FirstName is required.")
                .MaximumLength(16).WithMessage("FirstName cannot exceed 16 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("LastName is required.")
                .MaximumLength(16).WithMessage("LastName cannot exceed 16 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.")
                .MaximumLength(32).WithMessage("Email cannot exceed 32 characters.")
                .WithMessage("Email is already registered.");
        }
    }
}
