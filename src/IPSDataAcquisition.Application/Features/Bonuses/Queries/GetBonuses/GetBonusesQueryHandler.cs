using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IPSDataAcquisition.Application.Features.Bonuses.Queries.GetBonuses;

public class GetBonusesQueryHandler : IRequestHandler<GetBonusesQuery, List<BonusResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBonusesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BonusResponseDto>> Handle(GetBonusesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Bonuses.AsQueryable();

        // Default to last 30 days if no dates provided
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        query = query.Where(b => b.Date >= startDate && b.Date <= endDate);

        var bonuses = await query
            .OrderByDescending(b => b.Date)
            .Select(b => new BonusResponseDto(
                b.Date.ToString("yyyy-MM-dd"),
                b.Amount,
                b.SessionsCompleted,
                b.Description
            ))
            .ToListAsync(cancellationToken);

        return bonuses;
    }
}

