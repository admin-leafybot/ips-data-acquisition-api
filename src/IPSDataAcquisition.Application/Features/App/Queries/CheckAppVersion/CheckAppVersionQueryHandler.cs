using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IPSDataAcquisition.Application.Features.App.Queries.CheckAppVersion;

public class CheckAppVersionQueryHandler : IRequestHandler<CheckAppVersionQuery, CheckAppVersionResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CheckAppVersionQueryHandler> _logger;

    public CheckAppVersionQueryHandler(IApplicationDbContext context, ILogger<CheckAppVersionQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CheckAppVersionResponseDto> Handle(CheckAppVersionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking app version: {VersionName}", request.VersionName);

        var appVersion = await _context.AppVersions
            .FirstOrDefaultAsync(v => v.VersionName == request.VersionName, cancellationToken);

        if (appVersion == null)
        {
            _logger.LogWarning("App version not found in database: {VersionName}", request.VersionName);
            return new CheckAppVersionResponseDto(false);
        }

        _logger.LogInformation("App version {VersionName} found, Active: {Active}", 
            request.VersionName, appVersion.Active);

        return new CheckAppVersionResponseDto(appVersion.Active);
    }
}

