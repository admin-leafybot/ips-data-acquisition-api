using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IPSDataAcquisition.Application.Features.Sessions.Commands.CloseSession;

public class CloseSessionCommandHandler : IRequestHandler<CloseSessionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CloseSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CloseSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.SessionId == request.SessionId, cancellationToken);

        if (session == null)
        {
            throw new KeyNotFoundException($"Session with ID {request.SessionId} not found");
        }

        if (session.Status == SessionStatus.Completed)
        {
            throw new InvalidOperationException("Session is already closed");
        }

        if (request.EndTimestamp <= session.StartTimestamp)
        {
            throw new InvalidOperationException("End timestamp must be greater than start timestamp");
        }

        session.EndTimestamp = request.EndTimestamp;
        session.Status = SessionStatus.Completed;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

