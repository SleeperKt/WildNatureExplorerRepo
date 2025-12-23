using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WildNatureExplorer.Application.DTOs.Users;

public record UpdateUserDto(

    [Required]
    [Description("First name, Example = Alex")]
    string FirstName,

    [Required]
    [Description("Last name, Example = Black")]
    string LastName,

    [Required]
    [Description("User email, Example = alex2022@example.com")]
    string Email
);
