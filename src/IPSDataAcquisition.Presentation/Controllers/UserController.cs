using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Features.Users.Commands.Signup;
using IPSDataAcquisition.Application.Features.Users.Commands.Login;
using IPSDataAcquisition.Application.Features.Users.Commands.ChangeVerificationStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IPSDataAcquisition.Presentation.Controllers;

[ApiController]
[Route("api/v1/user")]
public class UserController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<UserController> _logger;

    public UserController(ISender sender, ILogger<UserController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<SignupResponseDto>> Signup(
        [FromBody] SignupRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new SignupCommand(request.Phone, request.Password, request.FullName);
            var result = await _sender.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup for phone: {Phone}", request.Phone);
            return StatusCode(500, new SignupResponseDto(false, "An error occurred during signup", null));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new LoginCommand(request.Phone, request.Password);
            var result = await _sender.Send(command, cancellationToken);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for phone: {Phone}", request.Phone);
            return StatusCode(500, new LoginResponseDto(false, "An error occurred during login", null, null, null));
        }
    }

    [HttpPost("ChangeVerificationStatus")]
    public async Task<ActionResult<ChangeVerificationStatusResponseDto>> ChangeVerificationStatus(
        [FromBody] ChangeVerificationStatusRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ChangeVerificationStatusCommand(request.Phone, request.Status);
            var result = await _sender.Send(command, cancellationToken);

            if (!result.Success)
            {
                // Check if it's a "not found" case
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing verification status for phone: {Phone}", request.Phone);
            return StatusCode(500, new ChangeVerificationStatusResponseDto(false, "An error occurred while updating verification status"));
        }
    }
}

