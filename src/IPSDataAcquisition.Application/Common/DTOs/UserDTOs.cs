namespace IPSDataAcquisition.Application.Common.DTOs;

public record SignupRequestDto(string Phone, string Password, string FullName);

public record SignupResponseDto(bool Success, string Message, string? UserId);

public record LoginRequestDto(string Phone, string Password);

public record LoginResponseDto(bool Success, string Message, string? Token, string? UserId, string? FullName);

public record ChangeVerificationStatusRequestDto(string Phone, bool Status);

public record ChangeVerificationStatusResponseDto(bool Success, string Message);

