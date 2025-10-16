using IPSDataAcquisition.Application.Common.DTOs;
using MediatR;

namespace IPSDataAcquisition.Application.Features.IMUData.Commands.UploadIMUData;

public record UploadIMUDataCommand(string? SessionId, List<IMUDataPointDto> DataPoints) : IRequest<UploadIMUDataResponseDto>;

