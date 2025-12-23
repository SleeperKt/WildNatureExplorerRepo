using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WildNatureExplorer.Application.DTOs.Auth;

public record RegisterUserDto(
    
    [Required]
    [Description("User Email, Example = alex2022@example.com")]
    string Email,

    [Required]
    [Description("Password, Example = Password123!")]
    string Password,

    [Required]
    [Description("First name, Example = Alex")]
    string FirstName,

    [Required]
    [Description("Last name, Example = Black")]
    string LastName
);
