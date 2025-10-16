using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Features.Bonuses.Queries.GetBonuses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IPSDataAcquisition.Presentation.Controllers;

[ApiController]
[Route("api/v1/bonuses")]
public class BonusesController : ControllerBase
{
    private readonly ISender _sender;

    public BonusesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BonusResponseDto>>>> GetBonuses(
        [FromQuery] string? start_date, [FromQuery] string? end_date, CancellationToken cancellationToken)
    {
        DateTime? startDate = null;
        DateTime? endDate = null;

        if (!string.IsNullOrEmpty(start_date) && DateTime.TryParse(start_date, out var parsedStart))
        {
            startDate = parsedStart;
        }

        if (!string.IsNullOrEmpty(end_date) && DateTime.TryParse(end_date, out var parsedEnd))
        {
            endDate = parsedEnd;
        }

        var result = await _sender.Send(new GetBonusesQuery(startDate, endDate), cancellationToken);

        return Ok(new ApiResponse<List<BonusResponseDto>>
        {
            Success = true,
            Data = result
        });
    }
}

