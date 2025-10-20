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
            var contentLength = Request.ContentLength ?? 0;
            var contentEncoding = Request.Headers["Content-Encoding"].ToString();
            
            _logger.LogInformation(
                "IMU Upload Request - Points: {PointCount}, Session: {SessionId}, ContentLength: {Length} bytes, Encoding: {Encoding}", 
                request.DataPoints?.Count ?? 0, 
                request.SessionId ?? "null",
                contentLength,
                string.IsNullOrEmpty(contentEncoding) ? "none" : contentEncoding);

            if (request.DataPoints == null || request.DataPoints.Count == 0)
            {
                _logger.LogWarning("IMU upload rejected - no data points provided");
                return BadRequest(new ApiResponse<UploadIMUDataResponseDto>
                {
                    Success = false,
                    Message = "data_points is required and must contain at least 1 data point",
                    Data = null
                });
            }

            // Log sample of first data point for debugging
            var firstPoint = request.DataPoints.First();
            _logger.LogDebug(
                "First IMU point - Timestamp: {Timestamp}, AccelX: {AccelX}, GyroX: {GyroX}, MagX: {MagX}",
                firstPoint.Timestamp, firstPoint.AccelX, firstPoint.GyroX, firstPoint.MagX);

            var result = await _sender.Send(
                new UploadIMUDataCommand(request.SessionId, request.DataPoints),
                cancellationToken);

            _logger.LogInformation("IMU data upload SUCCESS - {Count} points saved for session {SessionId}", 
                result.PointsReceived, result.SessionId ?? "null");

            return Ok(new ApiResponse<UploadIMUDataResponseDto>
            {
                Success = true,
                Message = "IMU data uploaded successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "IMU Upload FAILED - Session: {SessionId}, Points: {Count}, Error Type: {ErrorType}, Message: {Message}, StackTrace: {StackTrace}", 
                request.SessionId ?? "null",
                request.DataPoints?.Count ?? 0,
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace);
            
            return StatusCode(500, new ApiResponse<UploadIMUDataResponseDto>
            {
                Success = false,
                Message = $"Error uploading IMU data: {ex.Message}",
                Data = null
            });
        }
    }
}

