using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IPSDataAcquisition.Application.Features.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<RefreshTokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refresh token request received");

        // Find the refresh token
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Invalid refresh token");
            return new RefreshTokenResponseDto(false, "Invalid refresh token", null, null, null, null);
        }

        // Check if token is active
        if (!refreshToken.IsActive)
        {
            _logger.LogWarning("Refresh token is inactive or expired");
            return new RefreshTokenResponseDto(false, "Refresh token is expired or revoked", null, null, null, null);
        }

        var user = refreshToken.User;
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("User not found or inactive for refresh token");
            return new RefreshTokenResponseDto(false, "User account is not active", null, null, null, null);
        }

        // Generate new access token
        var tokenResult = _jwtTokenService.GenerateToken(user);

        // Generate new refresh token
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenEntity = new Domain.Entities.RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Revoke old refresh token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByToken = newRefreshToken;
        refreshToken.UpdatedAt = DateTime.UtcNow;

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token successful for user: {UserId}", user.Id);
        return new RefreshTokenResponseDto(
            true,
            "Token refreshed successfully",
            tokenResult.Token,
            newRefreshToken,
            tokenResult.ExpiresAt,
            tokenResult.ExpiresInSeconds);
    }
}

