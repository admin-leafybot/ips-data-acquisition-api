namespace IPSDataAcquisition.Application.Common.DTOs;

public record SignupRequestDto(string Phone, string Password, string FullName);

public record SignupResponseDto(bool Success, string Message, string? UserId);

public record LoginRequestDto(string Phone, string Password);

public record LoginResponseDto(
    bool Success, 
    string Message, 
    string? Token, 
    string? RefreshToken,
    string? UserId, 
    string? FullName,
    DateTime? ExpiresAt,
    int? ExpiresIn);

public record RefreshTokenRequestDto(string RefreshToken);

public record RefreshTokenResponseDto(
    bool Success,
    string Message,
    string? Token,
    string? RefreshToken,
    DateTime? ExpiresAt,
    int? ExpiresIn);

public record ChangeVerificationStatusRequestDto(string Phone, bool Status, string SecurityKey);

public record ChangeVerificationStatusResponseDto(bool Success, string Message);

