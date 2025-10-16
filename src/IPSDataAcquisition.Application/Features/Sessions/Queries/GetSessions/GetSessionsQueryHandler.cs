using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IPSDataAcquisition.Application.Features.Sessions.Queries.GetSessions;

public class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, List<SessionListResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSessionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SessionListResponseDto>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Min(request.Limit, 100);
        var skip = (request.Page - 1) * limit;

        var sessions = await _context.Sessions
            .OrderByDescending(s => s.StartTimestamp)
            .Skip(skip)
            .Take(limit)
            .Select(s => new SessionListResponseDto(
                s.SessionId,
                s.StartTimestamp,
                s.EndTimestamp,
                s.IsSynced,
                s.Status,
                s.PaymentStatus,
                s.Remarks,
                s.BonusAmount
            ))
            .ToListAsync(cancellationToken);

        return sessions;
    }
}

