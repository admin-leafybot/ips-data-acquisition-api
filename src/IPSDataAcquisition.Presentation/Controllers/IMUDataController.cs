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
    private readonly ILogger<IMUDataController> _logger;

    public IMUDataController(ISender sender, ILogger<IMUDataController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit for large IMU payloads
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<UploadIMUDataResponseDto>>> UploadIMUData(
        [FromBody] UploadIMUDataRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Received IMU data upload: {PointCount} points for session {SessionId}", 
                request.DataPoints?.Count ?? 0, 
                request.SessionId);

            if (request.DataPoints == null || request.DataPoints.Count == 0)
            {
                return BadRequest(new ApiResponse<UploadIMUDataResponseDto>
                {
                    Success = false,
                    Message = "data_points is required and must contain at least 1 data point",
                    Data = null
                });
            }

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
            _logger.LogError(ex, "Error uploading IMU data for session {SessionId}", request.SessionId);
            return StatusCode(500, new ApiResponse<UploadIMUDataResponseDto>
            {
                Success = false,
                Message = $"Error uploading IMU data: {ex.Message}",
                Data = null
            });
        }
    }
}

