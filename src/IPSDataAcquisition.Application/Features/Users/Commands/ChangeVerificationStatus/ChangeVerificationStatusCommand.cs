using IPSDataAcquisition.Application.Common.DTOs;
using MediatR;

namespace IPSDataAcquisition.Application.Features.Users.Commands.ChangeVerificationStatus;

public record ChangeVerificationStatusCommand(string Phone, bool Status) : IRequest<ChangeVerificationStatusResponseDto>;

