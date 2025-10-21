using MediatR;

namespace IPSDataAcquisition.Application.Features.Sessions.Commands.CancelSession;

public record CancelSessionCommand(string SessionId) : IRequest<bool>;

