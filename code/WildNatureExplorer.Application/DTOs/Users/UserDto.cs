namespace WildNatureExplorer.Application.DTOs.Users;

public record UserDto(Guid Id, string Email, string FirstName, string LastName, bool IsActive);
