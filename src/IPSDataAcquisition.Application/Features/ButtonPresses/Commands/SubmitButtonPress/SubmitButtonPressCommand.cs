using MediatR;

namespace IPSDataAcquisition.Application.Features.ButtonPresses.Commands.SubmitButtonPress;

public record SubmitButtonPressCommand(string SessionId, string Action, long Timestamp, int? FloorIndex) : IRequest<bool>;

