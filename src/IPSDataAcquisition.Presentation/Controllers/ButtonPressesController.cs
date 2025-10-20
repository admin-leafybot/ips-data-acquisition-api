using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Features.ButtonPresses.Commands.SubmitButtonPress;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IPSDataAcquisition.Presentation.Controllers;

[ApiController]
[Route("api/v1/button-presses")]
public class ButtonPressesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<ButtonPressesController> _logger;

    public ButtonPressesController(ISender sender, ILogger<ButtonPressesController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> SubmitButtonPress(
        [FromBody] SubmitButtonPressRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Button press: {Action} for session {SessionId} at {Timestamp}", 
                request.Action, request.SessionId, request.Timestamp);

            await _sender.Send(
                new SubmitButtonPressCommand(request.SessionId, request.Action, request.Timestamp, request.FloorIndex),
                cancellationToken);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Button press recorded",
                Data = null
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid button press action: {Action}", request.Action);
            
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Session not found: {SessionId}", request.SessionId);
            
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error submitting button press for session {SessionId}", 
                request.SessionId);
            throw;
        }
    }
}

