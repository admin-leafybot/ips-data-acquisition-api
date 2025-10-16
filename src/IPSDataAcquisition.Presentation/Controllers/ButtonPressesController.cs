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

    public ButtonPressesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> SubmitButtonPress(
        [FromBody] SubmitButtonPressRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(
                new SubmitButtonPressCommand(request.SessionId, request.Action, request.Timestamp),
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
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
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
    }
}

