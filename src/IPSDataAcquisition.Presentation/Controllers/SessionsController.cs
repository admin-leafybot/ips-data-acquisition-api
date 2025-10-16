using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Features.Sessions.Commands.CreateSession;
using IPSDataAcquisition.Application.Features.Sessions.Commands.CloseSession;
using IPSDataAcquisition.Application.Features.Sessions.Queries.GetSessions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IPSDataAcquisition.Presentation.Controllers;

[ApiController]
[Route("api/v1/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISender _sender;

    public SessionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<CreateSessionResponseDto>>> CreateSession(
        [FromBody] CreateSessionRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _sender.Send(
                new CreateSessionCommand(request.SessionId, request.Timestamp), 
                cancellationToken);

            return Ok(new ApiResponse<CreateSessionResponseDto>
            {
                Success = true,
                Message = "Session created successfully",
                Data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse<CreateSessionResponseDto>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
    }

    [HttpPost("close")]
    public async Task<ActionResult<ApiResponse<object>>> CloseSession(
        [FromBody] CloseSessionRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(
                new CloseSessionCommand(request.SessionId, request.EndTimestamp), 
                cancellationToken);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Session closed successfully",
                Data = null
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SessionListResponseDto>>>> GetSessions(
        [FromQuery] int page = 1, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetSessionsQuery(page, limit), cancellationToken);

        return Ok(new ApiResponse<List<SessionListResponseDto>>
        {
            Success = true,
            Data = result
        });
    }
}

