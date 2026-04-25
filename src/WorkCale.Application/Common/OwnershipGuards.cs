namespace WorkCale.Application.Common;

public static class OwnershipGuards
{
    public static T RequireOwned<T>(T? entity, Guid userId, Func<T, Guid> ownerSelector, string entityName)
        where T : class
    {
        if (entity is null)
            throw new KeyNotFoundException($"{entityName} not found.");
        if (ownerSelector(entity) != userId)
            throw new UnauthorizedAccessException($"You do not own this {entityName.ToLowerInvariant()}.");
        return entity;
    }
}
