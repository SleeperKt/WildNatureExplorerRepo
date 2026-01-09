using FluentValidation;
using WildNatureExplorer.Application.DTOs.Auth;
using WildNatureExplorer.Application.Interfaces.Services;

namespace WildNatureExplorer.Application.Validators
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator(IUserService userService)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.")
                .MaximumLength(32).WithMessage("Email cannot exceed 32 characters.")
                .WithMessage("Email is already registered.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .MaximumLength(20).WithMessage("Password cannot exceed 20 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"\d.*\d").WithMessage("Password must contain at least 2 digits.")
                .Matches(@"[!@#$?]").WithMessage("Password must contain at least one special character (! ? @ # $).");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("FirstName is required.")
                .MaximumLength(16).WithMessage("FirstName cannot exceed 16 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("LastName is required.")
                .MaximumLength(16).WithMessage("LastName cannot exceed 16 characters.");
        }
    }
}
