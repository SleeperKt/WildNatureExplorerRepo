using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using WildNatureExplorer.Infrastructure.Services;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class PasswordHasherTests
{
    private static string LegacyHash(string password)
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        var hash = KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            10_000,
            32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HashVerify_RoundTrip_V1Format()
    {
        var hasher = new PasswordHasher();
        var pwd = "Aa11!!aaaa";

        var stored = hasher.HashPassword(pwd);

        Assert.StartsWith("v1.", stored);
        Assert.True(hasher.VerifyPassword(pwd, stored));
        Assert.False(hasher.VerifyPassword("wrong", stored));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void VerifyPassword_LegacyTwoPartFormat_Works()
    {
        var hasher = new PasswordHasher();
        var pwd = "Aa11!!bbbb";
        var legacy = LegacyHash(pwd);

        Assert.True(hasher.VerifyPassword(pwd, legacy));
        Assert.False(hasher.VerifyPassword("nope", legacy));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void VerifyPassword_EmptyStored_ReturnsFalse()
    {
        var hasher = new PasswordHasher();
        Assert.False(hasher.VerifyPassword("x", ""));
        Assert.False(hasher.VerifyPassword("x", null!));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void VerifyPassword_GarbageStored_ReturnsFalse()
    {
        var hasher = new PasswordHasher();
        Assert.False(hasher.VerifyPassword("x", "not.valid.hash.format"));
    }
}
