using IPSDataAcquisition.Application.Common.DTOs;
using MediatR;

namespace IPSDataAcquisition.Application.Features.Sessions.Commands.CreateSession;

public record CreateSessionCommand(string SessionId, long Timestamp) : IRequest<CreateSessionResponseDto>;

