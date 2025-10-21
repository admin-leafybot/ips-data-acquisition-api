using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Features.Sessions.Commands.CreateSession;
using IPSDataAcquisition.Application.Features.Sessions.Commands.CloseSession;
using IPSDataAcquisition.Application.Features.Sessions.Commands.CancelSession;
using IPSDataAcquisition.Application.Features.Sessions.Queries.GetSessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPSDataAcquisition.Presentation.Controllers;

[ApiController]
[Route("api/v1/sessions")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(ISender sender, ILogger<SessionsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<CreateSessionResponseDto>>> CreateSession(
        [FromBody] CreateSessionRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating session: {SessionId} at timestamp {Timestamp}", 
                request.SessionId, request.Timestamp);

            var result = await _sender.Send(
                new CreateSessionCommand(request.SessionId, request.Timestamp), 
                cancellationToken);

            _logger.LogInformation("Session created successfully: {SessionId}", request.SessionId);

            return Ok(new ApiResponse<CreateSessionResponseDto>
            {
                Success = true,
                Message = "Session created successfully",
                Data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create session {SessionId}: {Message}", 
                request.SessionId, ex.Message);
            
            return Conflict(new ApiResponse<CreateSessionResponseDto>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating session {SessionId}", request.SessionId);
            throw;
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

    [HttpPost("cancel")]
    public async Task<ActionResult<ApiResponse<object>>> CancelSession(
        [FromBody] CancelSessionRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Cancelling session: {SessionId}", request.SessionId);

            await _sender.Send(
                new CancelSessionCommand(request.SessionId),
                cancellationToken);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Session cancelled successfully",
                Data = null
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Session not found for cancellation: {SessionId}", request.SessionId);
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized session cancellation attempt: {SessionId}", request.SessionId);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel session {SessionId}: {Message}", request.SessionId, ex.Message);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling session {SessionId}", request.SessionId);
            throw;
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

