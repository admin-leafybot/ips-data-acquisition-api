using IPSDataAcquisition.Application.Common.DTOs;
using MediatR;

namespace IPSDataAcquisition.Application.Features.Users.Commands.Signup;

public record SignupCommand(string Phone, string Password, string FullName) : IRequest<SignupResponseDto>;

