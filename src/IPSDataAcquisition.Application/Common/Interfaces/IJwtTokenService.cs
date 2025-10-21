using IPSDataAcquisition.Domain.Entities;

namespace IPSDataAcquisition.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user);
}

