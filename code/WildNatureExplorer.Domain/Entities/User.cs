using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class User : Entity
{
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

    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles;
    private readonly List<UserRole> _roles = new();

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
