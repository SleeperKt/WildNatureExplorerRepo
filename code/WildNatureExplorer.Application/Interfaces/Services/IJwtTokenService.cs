using System;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, IEnumerable<Role> roles);
}
