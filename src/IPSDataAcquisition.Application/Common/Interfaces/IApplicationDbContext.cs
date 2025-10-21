using IPSDataAcquisition.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IPSDataAcquisition.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Session> Sessions { get; }
    DbSet<ButtonPress> ButtonPresses { get; }
    DbSet<IMUData> IMUData { get; }
    DbSet<Bonus> Bonuses { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<AppVersion> AppVersions { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

