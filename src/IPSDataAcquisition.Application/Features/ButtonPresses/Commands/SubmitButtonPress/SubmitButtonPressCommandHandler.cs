using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IPSDataAcquisition.Application.Features.ButtonPresses.Commands.SubmitButtonPress;

public class SubmitButtonPressCommandHandler : IRequestHandler<SubmitButtonPressCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SubmitButtonPressCommandHandler> _logger;

    public SubmitButtonPressCommandHandler(IApplicationDbContext context, ILogger<SubmitButtonPressCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
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
            FloorIndex = request.FloorIndex,
            IsSynced = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Saving button press with FloorIndex: {FloorIndex} for session {SessionId}", 
            request.FloorIndex, request.SessionId);

        _context.ButtonPresses.Add(buttonPress);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

