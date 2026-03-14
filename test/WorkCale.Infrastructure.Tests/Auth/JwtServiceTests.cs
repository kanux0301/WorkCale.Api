using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using WorkCale.Domain.Entities;
using WorkCale.Infrastructure.Auth;

namespace WorkCale.Infrastructure.Tests.Auth;

public class JwtServiceTests
{
    private readonly JwtService _sut;
    private readonly User _user;

    public JwtServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-key-that-is-long-enough-for-hmac256",
                ["Jwt:Issuer"] = "workcale-test",
                ["Jwt:Audience"] = "workcale-client",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        _sut = new JwtService(config);
        _user = User.Create("alice@example.com", "Alice", "hash");
    }

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateAccessToken(_user);
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserIdClaim()
    {
        var token = _sut.GenerateAccessToken(_user);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == _user.Id.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ContainsEmailClaim()
    {
        var token = _sut.GenerateAccessToken(_user);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Email && c.Value == _user.Email);
    }

    [Fact]
    public void GenerateAccessToken_ContainsDisplayNameClaim()
    {
        var token = _sut.GenerateAccessToken(_user);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == "displayName" && c.Value == _user.DisplayName);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectIssuerAndAudience()
    {
        var token = _sut.GenerateAccessToken(_user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Issuer.Should().Be("workcale-test");
        jwt.Audiences.Should().Contain("workcale-client");
    }

    [Fact]
    public void GenerateAccessToken_ExpiresInConfiguredMinutes()
    {
        var token = _sut.GenerateAccessToken(_user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyBase64String()
    {
        var token = _sut.GenerateRefreshToken();
        token.Should().NotBeNullOrEmpty();
        var bytes = Convert.FromBase64String(token); // must not throw
        bytes.Should().HaveCount(64);
    }

    [Fact]
    public void GenerateRefreshToken_ProducesUniqueTokensEachCall()
    {
        var t1 = _sut.GenerateRefreshToken();
        var t2 = _sut.GenerateRefreshToken();
        t1.Should().NotBe(t2);
    }
}
