using IPSDataAcquisition.Application.Common.DTOs;
using MediatR;

namespace IPSDataAcquisition.Application.Features.Sessions.Queries.GetSessions;

public record GetSessionsQuery(int Page = 1, int Limit = 50) : IRequest<List<SessionListResponseDto>>;

