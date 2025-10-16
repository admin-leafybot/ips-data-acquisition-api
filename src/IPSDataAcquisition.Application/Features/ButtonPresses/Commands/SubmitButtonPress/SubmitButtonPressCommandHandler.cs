using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IPSDataAcquisition.Application.Features.ButtonPresses.Commands.SubmitButtonPress;

public class SubmitButtonPressCommandHandler : IRequestHandler<SubmitButtonPressCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public SubmitButtonPressCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(SubmitButtonPressCommand request, CancellationToken cancellationToken)
    {
        // Validate action
        if (!ButtonAction.ValidActions.Contains(request.Action))
        {
            throw new ArgumentException($"Invalid action: {request.Action}. Must be one of: {string.Join(", ", ButtonAction.ValidActions)}");
        }

        // Verify session exists
        var sessionExists = await _context.Sessions
            .AnyAsync(s => s.SessionId == request.SessionId, cancellationToken);

        if (!sessionExists)
        {
            throw new KeyNotFoundException($"Session with ID {request.SessionId} not found");
        }

        var buttonPress = new ButtonPress
        {
            SessionId = request.SessionId,
            Action = request.Action,
            Timestamp = request.Timestamp,
            IsSynced = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ButtonPresses.Add(buttonPress);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

