using FluentAssertions;
using WorkCale.Application.Common;
using Xunit;

namespace WorkCale.Application.Tests.Common;

public class OwnershipGuardsTests
{
    private record Owned(Guid OwnerId);

    [Fact]
    public void RequireOwned_NullEntity_ThrowsKeyNotFound()
    {
        Action act = () => OwnershipGuards.RequireOwned<Owned>(null, Guid.NewGuid(), e => e.OwnerId, "Shift");

        act.Should().Throw<KeyNotFoundException>().WithMessage("Shift not found.");
    }

    [Fact]
    public void RequireOwned_DifferentOwner_ThrowsUnauthorized()
    {
        var entity = new Owned(Guid.NewGuid());

        Action act = () => OwnershipGuards.RequireOwned(entity, Guid.NewGuid(), e => e.OwnerId, "Category");

        act.Should().Throw<UnauthorizedAccessException>().WithMessage("You do not own this category.");
    }

    [Fact]
    public void RequireOwned_SameOwner_ReturnsEntity()
    {
        var userId = Guid.NewGuid();
        var entity = new Owned(userId);

        var result = OwnershipGuards.RequireOwned(entity, userId, e => e.OwnerId, "Shift");

        result.Should().BeSameAs(entity);
    }
}
