using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class User : Entity
{
    private readonly List<UserRole> _userRoles = new();

    private User() { }

    public User(
        Guid id,
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool AcceptedTerms { get; private set; }
    public DateTime? AcceptedTermsAt { get; private set; }
    public string? TermsVersion { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

    public void AddRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id))
            return;

        _userRoles.Add(new UserRole(Id, role.Id));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AcceptTerms(string version)
    {
        AcceptedTerms = true;
        AcceptedTermsAt = DateTime.UtcNow;
        TermsVersion = version;
    }
}
