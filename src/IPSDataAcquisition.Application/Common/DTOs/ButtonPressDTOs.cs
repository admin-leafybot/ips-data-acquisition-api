namespace IPSDataAcquisition.Application.Common.DTOs;

public record SubmitButtonPressRequestDto(string SessionId, string Action, long Timestamp, int? FloorIndex);

