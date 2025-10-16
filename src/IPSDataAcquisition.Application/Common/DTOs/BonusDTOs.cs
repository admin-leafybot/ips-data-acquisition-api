namespace IPSDataAcquisition.Application.Common.DTOs;

public record BonusResponseDto(string Date, decimal Amount, int SessionsCompleted, string? Description);

