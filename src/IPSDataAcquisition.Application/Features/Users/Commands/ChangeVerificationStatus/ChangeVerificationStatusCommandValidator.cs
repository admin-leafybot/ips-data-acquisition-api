using FluentValidation;

namespace IPSDataAcquisition.Application.Features.Users.Commands.ChangeVerificationStatus;

public class ChangeVerificationStatusCommandValidator : AbstractValidator<ChangeVerificationStatusCommand>
{
    public ChangeVerificationStatusCommandValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .MinimumLength(10).WithMessage("Phone number must be at least 10 digits")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.SecurityKey)
            .NotEmpty().WithMessage("Security key is required");

        // Status is a boolean, so no validation needed beyond the type itself
    }
}

