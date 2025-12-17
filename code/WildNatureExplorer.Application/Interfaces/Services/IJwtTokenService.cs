using System;

namespace WildNatureExplorer.Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email);
}
