using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Domain.Entities;

public class Role : Entity
{
    private Role() { }

    public Role(Guid id, string roleName, string description)
    {
        Id = id;
        RoleName = roleName;
        Description = description;
    }

    public string RoleName { get; private set; }   // admin, user, support
    public string Description { get; private set; }

    public IReadOnlyCollection<UserRole> Users => _users;
    private readonly List<UserRole> _users = new();
}
