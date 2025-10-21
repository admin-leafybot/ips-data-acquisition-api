using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IPSDataAcquisition.Application.Features.Sessions.Commands.CancelSession;

public class CancelSessionCommandHandler : IRequestHandler<CancelSessionCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CancelSessionCommandHandler> _logger;

    public CancelSessionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CancelSessionCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelSessionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling session: {SessionId} for user {UserId}", 
            request.SessionId, _currentUserService.UserId);

        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.SessionId == request.SessionId, cancellationToken);

        if (session == null)
        {
            _logger.LogWarning("Session not found: {SessionId}", request.SessionId);
            throw new KeyNotFoundException($"Session with ID {request.SessionId} not found");
        }

        // Verify session belongs to current user
        if (session.UserId != _currentUserService.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to cancel session {SessionId} owned by {OwnerId}", 
                _currentUserService.UserId, request.SessionId, session.UserId);
            throw new UnauthorizedAccessException("You can only cancel your own sessions");
        }

        // Check if session is already in a final state
        if (session.Status == SessionStatus.Completed || 
            session.Status == SessionStatus.Approved || 
            session.Status == SessionStatus.Rejected)
        {
            _logger.LogWarning("Cannot cancel session {SessionId} with status {Status}", 
                request.SessionId, session.Status);
            throw new InvalidOperationException($"Cannot cancel session with status: {session.Status}");
        }

        // Update session status to rejected (cancelled)
        session.Status = SessionStatus.Rejected;
        session.EndTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        session.Remarks = "Cancelled by user";
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Session cancelled successfully: {SessionId}", request.SessionId);
        return true;
    }
}

