using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IPSDataAcquisition.Application.Features.Users.Commands.ChangeVerificationStatus;

public class ChangeVerificationStatusCommandHandler : IRequestHandler<ChangeVerificationStatusCommand, ChangeVerificationStatusResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminSecurityService _adminSecurityService;
    private readonly ILogger<ChangeVerificationStatusCommandHandler> _logger;

    public ChangeVerificationStatusCommandHandler(
        UserManager<ApplicationUser> userManager,
        IAdminSecurityService adminSecurityService,
        ILogger<ChangeVerificationStatusCommandHandler> logger)
    {
        _userManager = userManager;
        _adminSecurityService = adminSecurityService;
        _logger = logger;
    }

    public async Task<ChangeVerificationStatusResponseDto> Handle(ChangeVerificationStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Change verification status request for phone: {Phone}, Status: {Status}", request.Phone, request.Status);

        // Validate security key
        if (!_adminSecurityService.ValidateChangeVerificationSecurityKey(request.SecurityKey))
        {
            _logger.LogWarning("Invalid security key provided for phone: {Phone}", request.Phone);
            return new ChangeVerificationStatusResponseDto(false, "Invalid security key");
        }

        // Find user by phone number
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.Phone, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found for phone: {Phone}", request.Phone);
            return new ChangeVerificationStatusResponseDto(false, "User not found");
        }

        // Update IsActive status
        user.IsActive = request.Status;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            var statusText = request.Status ? "activated" : "deactivated";
            _logger.LogInformation("User account {Status} for phone: {Phone}", statusText, request.Phone);
            return new ChangeVerificationStatusResponseDto(true, $"User account {statusText} successfully");
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogWarning("Failed to update verification status for phone {Phone}: {Errors}", request.Phone, errors);
        return new ChangeVerificationStatusResponseDto(false, $"Failed to update user: {errors}");
    }
}

