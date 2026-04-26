using FluentAssertions;
using WorkCale.Application.Common;
using Xunit;

namespace WorkCale.Application.Tests.Common;

public class TimeFormatsTests
{
    [Theory]
    [InlineData("00:00", 0, 0)]
    [InlineData("09:30", 9, 30)]
    [InlineData("23:59", 23, 59)]
    public void ParseHHmm_Valid_Parses(string input, int hour, int minute)
    {
        var result = TimeFormats.ParseHHmm(input);

        result.Hour.Should().Be(hour);
        result.Minute.Should().Be(minute);
    }

    [Theory]
    [InlineData("9:30")]
    [InlineData("24:00")]
    [InlineData("not-a-time")]
    [InlineData("")]
    public void ParseHHmm_Invalid_Throws(string input)
    {
        Action act = () => TimeFormats.ParseHHmm(input);

        act.Should().Throw<FormatException>();
    }
}
