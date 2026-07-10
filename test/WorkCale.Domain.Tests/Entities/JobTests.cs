using FluentAssertions;
using WorkCale.Domain.Entities;
using Xunit;

namespace WorkCale.Domain.Tests.Entities;

public class JobTests
{
    [Fact]
    public void Create_PopulatesFields_AndDefaultsToNotArchived()
    {
        var job = Job.Create(Guid.NewGuid(), "Hospital A", "#4C6FA3", "briefcase-outline", isDefault: true);

        job.Name.Should().Be("Hospital A");
        job.Color.Should().Be("#4C6FA3");
        job.Icon.Should().Be("briefcase-outline");
        job.IsDefault.Should().BeTrue();
        job.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Update_ReplacesNameColorIcon()
    {
        var job = Job.Create(Guid.NewGuid(), "Old", "#111111", "old-icon");

        job.Update("New", "#ABCDEF", "new-icon");

        job.Name.Should().Be("New");
        job.Color.Should().Be("#ABCDEF");
        job.Icon.Should().Be("new-icon");
    }

    [Fact]
    public void Archive_OnNonDefault_MarksArchived()
    {
        var job = Job.Create(Guid.NewGuid(), "Side gig", "#EC4899");

        job.Archive();

        job.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Archive_OnDefault_Throws()
    {
        var job = Job.Create(Guid.NewGuid(), "My Job", "#4C6FA3", isDefault: true);

        Action act = () => job.Archive();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*default job*");
    }

    [Fact]
    public void MakeDefault_And_ClearDefault_ToggleFlag()
    {
        var job = Job.Create(Guid.NewGuid(), "A", "#111111");

        job.MakeDefault();
        job.IsDefault.Should().BeTrue();

        job.ClearDefault();
        job.IsDefault.Should().BeFalse();
    }
}
