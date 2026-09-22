using BreakdownManager.Domain.Security;
using Xunit;

namespace BreakdownManager.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_Succeeds()
    {
        var hash = PasswordHasher.Hash("correct-horse-battery-staple");

        Assert.True(PasswordHasher.Verify("correct-horse-battery-staple", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_Fails()
    {
        var hash = PasswordHasher.Hash("correct-horse-battery-staple");

        Assert.False(PasswordHasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_NeverProducesPlaintextOrTheSameOutputTwice()
    {
        var first = PasswordHasher.Hash("jabu");
        var second = PasswordHasher.Hash("jabu");

        // Different random salt each time, so two hashes of the same password never match byte-for-byte...
        Assert.NotEqual(first, second);
        Assert.DoesNotContain("jabu", first);

        // ...but both still verify correctly against the original password.
        Assert.True(PasswordHasher.Verify("jabu", first));
        Assert.True(PasswordHasher.Verify("jabu", second));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-valid-hash")]
    [InlineData("100000.not-base64!!.also-not-base64!!")]
    public void Verify_WithMalformedHash_ReturnsFalseRatherThanThrowing(string malformedHash)
    {
        Assert.False(PasswordHasher.Verify("anything", malformedHash));
    }
}
