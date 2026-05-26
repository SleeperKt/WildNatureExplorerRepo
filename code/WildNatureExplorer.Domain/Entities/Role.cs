using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class Role : Entity
{
    private Role() { }

    public Role(Guid id, string roleName, string description)
    {
        Id = id;
        RoleName = roleName ?? string.Empty;
        Description = description ?? string.Empty;
    }

    public string RoleName { get; private set; } = string.Empty;   // admin, user, support
    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<UserRole> Users => _users;
    private readonly List<UserRole> _users = new();
}
