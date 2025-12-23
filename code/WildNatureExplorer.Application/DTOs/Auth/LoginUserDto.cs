using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WildNatureExplorer.Application.DTOs.Auth;

public record LoginUserDto(
    
    [Required]
    [Description("User email, Example = alex2022@example.com")]
    string Email,

    [Required]
    [Description("Password, Example = Password123!")]
    string Password
);