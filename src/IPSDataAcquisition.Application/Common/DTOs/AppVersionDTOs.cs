namespace IPSDataAcquisition.Application.Common.DTOs;

public record CheckAppVersionRequestDto(string VersionName);

public record CheckAppVersionResponseDto(bool IsActive);

