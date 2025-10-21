using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IPSDataAcquisition.Application.Features.Users.Commands.Signup;

public class SignupCommandHandler : IRequestHandler<SignupCommand, SignupResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SignupCommandHandler> _logger;

    public SignupCommandHandler(UserManager<ApplicationUser> userManager, ILogger<SignupCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<SignupResponseDto> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Signup request for phone: {Phone}", request.Phone);

        // Check if user already exists
        var existingUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.Phone, cancellationToken);

        if (existingUser != null)
        {
            _logger.LogWarning("Phone number already registered: {Phone}", request.Phone);
            return new SignupResponseDto(false, "Phone number already registered", null);
        }

        // Create new user
        var user = new ApplicationUser
        {
            UserName = request.Phone, // Use phone as username
            PhoneNumber = request.Phone,
            FullName = request.FullName,
            IsActive = false, // User is not active until verified
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User created successfully: {UserId}, Phone: {Phone}", user.Id, request.Phone);
            return new SignupResponseDto(true, "User registered successfully. Account is pending verification.", user.Id);
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogWarning("User creation failed for phone {Phone}: {Errors}", request.Phone, errors);
        return new SignupResponseDto(false, $"Failed to create user: {errors}", null);
    }
}

