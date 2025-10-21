using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IPSDataAcquisition.Application.Features.Users.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for phone: {Phone}", request.Phone);

        // Find user by phone number
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.Phone, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found for phone {Phone}", request.Phone);
            return new LoginResponseDto(false, "Invalid phone number or password", null, null, null);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: User account not active for phone {Phone}", request.Phone);
            return new LoginResponseDto(false, "Your account is not yet verified. Please contact administrator.", null, null, null);
        }

        // Validate password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed: Invalid password for phone {Phone}", request.Phone);
            return new LoginResponseDto(false, "Invalid phone number or password", null, null, null);
        }

        // Generate JWT token
        var token = _jwtTokenService.GenerateToken(user);

        _logger.LogInformation("User logged in successfully: {UserId}, Phone: {Phone}", user.Id, request.Phone);
        return new LoginResponseDto(true, "Login successful", token, user.Id, user.FullName);
    }
}

