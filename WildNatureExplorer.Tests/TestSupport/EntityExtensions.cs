using System.Reflection;
using WildNatureExplorer.Domain.Base;

namespace WildNatureExplorer.Tests.TestSupport;

/// <summary>Assigns <see cref="Entity.Id"/> for in-memory tests (production IDs come from EF).</summary>
public static class EntityExtensions
{
    private static readonly PropertyInfo IdProp =
        typeof(Entity).GetProperty(nameof(Entity.Id))!;

    public static T WithTestId<T>(this T entity, Guid id) where T : Entity
    {
        IdProp.SetValue(entity, id);
        return entity;
    }
}
