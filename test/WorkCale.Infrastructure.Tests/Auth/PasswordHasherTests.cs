using FluentAssertions;
using WorkCale.Infrastructure.Auth;

namespace WorkCale.Infrastructure.Tests.Auth;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = _sut.Hash("mypassword");
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_ProducesDifferentHashEachCall()
    {
        var h1 = _sut.Hash("same");
        var h2 = _sut.Hash("same");
        h1.Should().NotBe(h2); // BCrypt uses random salt
    }

    [Fact]
    public void Hash_DoesNotReturnPlaintext()
    {
        var hash = _sut.Hash("secret");
        hash.Should().NotBe("secret");
    }

    [Fact]
    public void Verify_ReturnsTrue_ForMatchingPassword()
    {
        var hash = _sut.Hash("correct");
        _sut.Verify("correct", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hash = _sut.Hash("correct");
        _sut.Verify("wrong", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_ReturnsFalse_ForEmptyPassword()
    {
        var hash = _sut.Hash("correct");
        _sut.Verify("", hash).Should().BeFalse();
    }

    [Fact]
    public void HashAndVerify_WorkForSpecialCharacters()
    {
        const string password = "P@$$w0rd!#&*()";
        var hash = _sut.Hash(password);
        _sut.Verify(password, hash).Should().BeTrue();
    }
}
