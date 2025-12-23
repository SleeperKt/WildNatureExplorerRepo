using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.Extensions.Configuration;
using WildNatureExplorer.Infrastructure.Services;

namespace WildNatureExplorer.Tests.Unit
{
    public class JwtTokenServiceTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void GenerateToken_ReturnsNonEmptyToken()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"JWT_KEY", "K4t5h9wL8fJ2qP1vZ3yX8rQ0sM7bN6pD"},
                {"JWT_ISSUER", "TestIssuer"},
                {"JWT_AUDIENCE", "TestAudience"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new JwtTokenService(configuration);
            var token = service.GenerateToken(Guid.NewGuid(), "test@example.com");

            Assert.False(string.IsNullOrEmpty(token));
        }
    }
}
