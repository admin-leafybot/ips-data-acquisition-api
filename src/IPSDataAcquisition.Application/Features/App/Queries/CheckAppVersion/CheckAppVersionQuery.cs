using IPSDataAcquisition.Application.Common.DTOs;
using MediatR;

namespace IPSDataAcquisition.Application.Features.App.Queries.CheckAppVersion;

public record CheckAppVersionQuery(string VersionName) : IRequest<CheckAppVersionResponseDto>;

