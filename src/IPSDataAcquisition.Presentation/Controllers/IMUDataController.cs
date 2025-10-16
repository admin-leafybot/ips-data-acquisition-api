using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Features.IMUData.Commands.UploadIMUData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IPSDataAcquisition.Presentation.Controllers;

[ApiController]
[Route("api/v1/imu-data")]
public class IMUDataController : ControllerBase
{
    private readonly ISender _sender;

    public IMUDataController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<UploadIMUDataResponseDto>>> UploadIMUData(
        [FromBody] UploadIMUDataRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _sender.Send(
                new UploadIMUDataCommand(request.SessionId, request.DataPoints),
                cancellationToken);

            return Ok(new ApiResponse<UploadIMUDataResponseDto>
            {
                Success = true,
                Message = "IMU data uploaded successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<UploadIMUDataResponseDto>
            {
                Success = false,
                Message = $"Error uploading IMU data: {ex.Message}",
                Data = null
            });
        }
    }
}

