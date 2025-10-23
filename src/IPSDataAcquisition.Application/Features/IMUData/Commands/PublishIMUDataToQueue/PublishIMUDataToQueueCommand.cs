using IPSDataAcquisition.Application.Common.DTOs;
using MediatR;

namespace IPSDataAcquisition.Application.Features.IMUData.Commands.PublishIMUDataToQueue;

public record PublishIMUDataToQueueCommand(string? SessionId, List<IMUDataPointDto> DataPoints) 
    : IRequest<UploadIMUDataResponseDto>;

