using FluentValidation;

namespace IPSDataAcquisition.Application.Features.App.Queries.CheckAppVersion;

public class CheckAppVersionQueryValidator : AbstractValidator<CheckAppVersionQuery>
{
    public CheckAppVersionQueryValidator()
    {
        RuleFor(x => x.VersionName)
            .NotEmpty().WithMessage("Version name is required")
            .MaximumLength(50).WithMessage("Version name must not exceed 50 characters");
    }
}

