using IPSDataAcquisition.Application.Common.DTOs;
using MediatR;

namespace IPSDataAcquisition.Application.Features.Bonuses.Queries.GetBonuses;

public record GetBonusesQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<List<BonusResponseDto>>;

