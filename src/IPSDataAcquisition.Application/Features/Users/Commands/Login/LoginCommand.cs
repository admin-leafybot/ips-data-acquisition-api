using IPSDataAcquisition.Application.Common.DTOs;
using MediatR;

namespace IPSDataAcquisition.Application.Features.Users.Commands.Login;

public record LoginCommand(string Phone, string Password) : IRequest<LoginResponseDto>;

