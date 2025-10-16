namespace IPSDataAcquisition.Application.Common.DTOs;

public record CreateSessionRequestDto(string SessionId, long Timestamp);

public record CreateSessionResponseDto(string SessionId, long CreatedAt);

public record CloseSessionRequestDto(string SessionId, long EndTimestamp);

public record SessionListResponseDto(
    string SessionId,
    long StartTimestamp,
    long? EndTimestamp,
    bool IsSynced,
    string Status,
    string PaymentStatus,
    string? Remarks,
    decimal? BonusAmount
);

