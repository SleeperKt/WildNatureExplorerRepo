using Xunit;
using Microsoft.AspNetCore.Identity;
using WildNatureExplorer.Domain.Entities;
using System;

namespace WildNatureExplorer.Tests.Unit
{
    public class PasswordHasherTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void HashPassword_ProducesValidHash()
        {
            var hasher = new PasswordHasher<User>();
            var user = new User(
                Guid.NewGuid(),
                "test@example.com",
                "FirstName",
                "LastName",
                "InitialPassword"
            );
            var password = "MySecret123!";

            var hashed = hasher.HashPassword(user, password);

            var result = hasher.VerifyHashedPassword(user, hashed, password);

            Assert.Equal(PasswordVerificationResult.Success, result);
        }
    }
}
