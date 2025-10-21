using FluentValidation;

namespace IPSDataAcquisition.Application.Features.Sessions.Commands.CancelSession;

public class CancelSessionCommandValidator : AbstractValidator<CancelSessionCommand>
{
    public CancelSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required")
            .MaximumLength(36).WithMessage("Session ID must not exceed 36 characters");
    }
}

