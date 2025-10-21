using IPSDataAcquisition.Domain.Entities;

namespace IPSDataAcquisition.Application.Common.Interfaces;

public interface IJwtTokenService
{
    TokenResult GenerateToken(ApplicationUser user);
    string GenerateRefreshToken();
}

public record TokenResult(string Token, DateTime ExpiresAt, int ExpiresInSeconds);

