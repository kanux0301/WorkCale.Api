using FluentAssertions;
using FluentValidation;
using WorkCale.Application.Common;
using Xunit;

namespace WorkCale.Application.Tests.Common;

public class ValidationExtensionsTests
{
    private record EmailReq(string Email);
    private record TimeReq(string? Time);
    private record ColorReq(string Color);

    private class EmailValidator : AbstractValidator<EmailReq>
    {
        public EmailValidator() => RuleFor(x => x.Email).ValidEmail();
    }

    private class TimeValidator : AbstractValidator<TimeReq>
    {
        public TimeValidator() => RuleFor(x => x.Time).HHmmFormat("Time").When(x => x.Time != null);
    }

    private class ColorValidator : AbstractValidator<ColorReq>
    {
        public ColorValidator() => RuleFor(x => x.Color).HexColor();
    }

    [Theory]
    [InlineData("a@b.com", true)]
    [InlineData("user+tag@example.co.uk", true)]
    [InlineData("", false)]
    [InlineData("not-an-email", false)]
    public void ValidEmail_ChecksFormat(string input, bool expected)
    {
        var result = new EmailValidator().Validate(new EmailReq(input));

        result.IsValid.Should().Be(expected);
    }

    [Fact]
    public void ValidEmail_TooLong_Fails()
    {
        var longLocal = new string('a', 250);
        var result = new EmailValidator().Validate(new EmailReq($"{longLocal}@b.com"));

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("00:00", true)]
    [InlineData("23:59", true)]
    [InlineData("9:30", false)]
    [InlineData("not-a-time", false)]
    public void HHmmFormat_ChecksFormat(string input, bool expected)
    {
        var result = new TimeValidator().Validate(new TimeReq(input));

        result.IsValid.Should().Be(expected);
    }

    [Fact]
    public void HHmmFormat_NullSkipped_WhenWhenClause()
    {
        var result = new TimeValidator().Validate(new TimeReq(null));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("#F59E0B", true)]
    [InlineData("#abcdef", true)]
    [InlineData("#ABC", false)]
    [InlineData("F59E0B", false)]
    [InlineData("", false)]
    public void HexColor_ChecksFormat(string input, bool expected)
    {
        var result = new ColorValidator().Validate(new ColorReq(input));

        result.IsValid.Should().Be(expected);
    }
}
