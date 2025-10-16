using MediatR;

namespace IPSDataAcquisition.Application.Features.Sessions.Commands.CloseSession;

public record CloseSessionCommand(string SessionId, long EndTimestamp) : IRequest<bool>;

