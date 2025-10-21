using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Features.App.Queries.CheckAppVersion;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IPSDataAcquisition.Presentation.Controllers;

[ApiController]
[Route("api/v1/app")]
public class AppController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<AppController> _logger;

    public AppController(ISender sender, ILogger<AppController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpPost("checkAppVersion")]
    public async Task<ActionResult<CheckAppVersionResponseDto>> CheckAppVersion(
        [FromBody] CheckAppVersionRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new CheckAppVersionQuery(request.VersionName);
            var result = await _sender.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking app version: {VersionName}", request.VersionName);
            return StatusCode(500, new CheckAppVersionResponseDto(false));
        }
    }
}

