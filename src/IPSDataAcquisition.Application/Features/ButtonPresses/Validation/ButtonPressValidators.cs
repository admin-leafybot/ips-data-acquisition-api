using FluentValidation;
using IPSDataAcquisition.Application.Features.ButtonPresses.Commands.SubmitButtonPress;
using IPSDataAcquisition.Domain.Entities;

namespace IPSDataAcquisition.Application.Features.ButtonPresses.Validation;

public class SubmitButtonPressCommandValidator : AbstractValidator<SubmitButtonPressCommand>
{
    public SubmitButtonPressCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("session_id is required");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("action is required")
            .Must(action => ButtonAction.ValidActions.Contains(action))
            .WithMessage($"action must be one of: {string.Join(", ", ButtonAction.ValidActions)}");

        RuleFor(x => x.Timestamp)
            .GreaterThan(0).WithMessage("timestamp must be a positive number");
    }
}

