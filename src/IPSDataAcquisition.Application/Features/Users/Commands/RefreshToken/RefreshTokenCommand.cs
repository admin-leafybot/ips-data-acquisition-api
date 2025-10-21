using IPSDataAcquisition.Application.Common.DTOs;
using MediatR;

namespace IPSDataAcquisition.Application.Features.Users.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponseDto>;

