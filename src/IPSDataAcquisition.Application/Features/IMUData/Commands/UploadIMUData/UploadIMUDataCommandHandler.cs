using IPSDataAcquisition.Application.Common.DTOs;
using IPSDataAcquisition.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IPSDataAcquisition.Application.Features.IMUData.Commands.UploadIMUData;

public class UploadIMUDataCommandHandler : IRequestHandler<UploadIMUDataCommand, UploadIMUDataResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UploadIMUDataCommandHandler> _logger;

    public UploadIMUDataCommandHandler(IApplicationDbContext context, ILogger<UploadIMUDataCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UploadIMUDataResponseDto> Handle(UploadIMUDataCommand request, CancellationToken cancellationToken)
    {
        var imuDataList = new List<Domain.Entities.IMUData>();

        foreach (var point in request.DataPoints)
        {
            var imuData = new Domain.Entities.IMUData
            {
                SessionId = request.SessionId,
                Timestamp = point.Timestamp,
                // Basic calibrated sensors - all nullable
                AccelX = point.AccelX, AccelY = point.AccelY, AccelZ = point.AccelZ,
                GyroX = point.GyroX, GyroY = point.GyroY, GyroZ = point.GyroZ,
                MagX = point.MagX, MagY = point.MagY, MagZ = point.MagZ,
                GravityX = point.GravityX, GravityY = point.GravityY, GravityZ = point.GravityZ,
                LinearAccelX = point.LinearAccelX, LinearAccelY = point.LinearAccelY, LinearAccelZ = point.LinearAccelZ,
                AccelUncalX = point.AccelUncalX, AccelUncalY = point.AccelUncalY, AccelUncalZ = point.AccelUncalZ,
                AccelBiasX = point.AccelBiasX, AccelBiasY = point.AccelBiasY, AccelBiasZ = point.AccelBiasZ,
                GyroUncalX = point.GyroUncalX, GyroUncalY = point.GyroUncalY, GyroUncalZ = point.GyroUncalZ,
                GyroDriftX = point.GyroDriftX, GyroDriftY = point.GyroDriftY, GyroDriftZ = point.GyroDriftZ,
                MagUncalX = point.MagUncalX, MagUncalY = point.MagUncalY, MagUncalZ = point.MagUncalZ,
                MagBiasX = point.MagBiasX, MagBiasY = point.MagBiasY, MagBiasZ = point.MagBiasZ,
                RotationVectorX = point.RotationVectorX, RotationVectorY = point.RotationVectorY,
                RotationVectorZ = point.RotationVectorZ, RotationVectorW = point.RotationVectorW,
                GameRotationX = point.GameRotationX, GameRotationY = point.GameRotationY,
                GameRotationZ = point.GameRotationZ, GameRotationW = point.GameRotationW,
                GeomagRotationX = point.GeomagRotationX, GeomagRotationY = point.GeomagRotationY,
                GeomagRotationZ = point.GeomagRotationZ, GeomagRotationW = point.GeomagRotationW,
                Pressure = point.Pressure, Temperature = point.Temperature, Light = point.Light,
                Humidity = point.Humidity, Proximity = point.Proximity,
                StepCounter = point.StepCounter, StepDetected = point.StepDetected,
                Roll = point.Roll, Pitch = point.Pitch, Yaw = point.Yaw, Heading = point.Heading,
                Latitude = point.Latitude, Longitude = point.Longitude, Altitude = point.Altitude,
                GpsAccuracy = point.GpsAccuracy, Speed = point.Speed,
                IsSynced = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            imuDataList.Add(imuData);
        }

        // Bulk insert for performance
        await _context.IMUData.AddRangeAsync(imuDataList, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully processed {Count} IMU data points for session {SessionId}",
            imuDataList.Count, request.SessionId);

        return new UploadIMUDataResponseDto(imuDataList.Count, request.SessionId);
    }
}

