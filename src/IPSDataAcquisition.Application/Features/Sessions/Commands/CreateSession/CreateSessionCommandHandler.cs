using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IPSDataAcquisition.Application.Features.Sessions.Commands.CreateSession;

public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, CreateSessionResponseDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateSessionResponseDto> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        // Check if session already exists
        var exists = await _context.Sessions
            .AnyAsync(s => s.SessionId == request.SessionId, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Session with ID {request.SessionId} already exists");
        }

        var session = new Session
        {
            SessionId = request.SessionId,
            StartTimestamp = request.Timestamp,
            Status = SessionStatus.InProgress,
            PaymentStatus = PaymentStatusEnum.Unpaid,
            IsSynced = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateSessionResponseDto(session.SessionId, session.StartTimestamp);
    }
}

