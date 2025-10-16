using FluentValidation;
using IPSDataAcquisition.Application.Features.Sessions.Commands.CreateSession;
using IPSDataAcquisition.Application.Features.Sessions.Commands.CloseSession;

namespace IPSDataAcquisition.Application.Features.Sessions.Validation;

public class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("session_id is required")
            .Matches(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
            .WithMessage("session_id must be a valid UUID");

        RuleFor(x => x.Timestamp)
            .GreaterThan(0).WithMessage("timestamp must be a positive number");
    }
}

public class CloseSessionCommandValidator : AbstractValidator<CloseSessionCommand>
{
    public CloseSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("session_id is required");

        RuleFor(x => x.EndTimestamp)
            .GreaterThan(0).WithMessage("end_timestamp must be a positive number");
    }
}

