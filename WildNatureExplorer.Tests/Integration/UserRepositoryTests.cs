using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Infrastructure.Data;
using WildNatureExplorer.Infrastructure.Repositories;
using WildNatureExplorer.Domain.Entities;
using Xunit;

namespace WildNatureExplorer.Tests.Integration
{
    public class UserRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task AddUser_Should_AddUserToDatabase()
        {
            var context = GetDbContext();
            var repo = new UserRepository(context);
            var user = new User(
                Guid.NewGuid(),
                "test@example.com",
                "FirstName",
                "LastName",
                "PasswordHash"
            );

            await repo.AddAsync(user);
            await context.SaveChangesAsync();

            var userFromDb = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.NotNull(userFromDb);
        }
    }
}
