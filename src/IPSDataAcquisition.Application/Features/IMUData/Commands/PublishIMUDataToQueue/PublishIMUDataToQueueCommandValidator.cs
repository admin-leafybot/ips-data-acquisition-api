using FluentValidation;

namespace IPSDataAcquisition.Application.Features.IMUData.Commands.PublishIMUDataToQueue;

public class PublishIMUDataToQueueCommandValidator : AbstractValidator<PublishIMUDataToQueueCommand>
{
    public PublishIMUDataToQueueCommandValidator()
    {
        RuleFor(x => x.DataPoints)
            .NotNull().WithMessage("DataPoints is required")
            .NotEmpty().WithMessage("DataPoints must contain at least 1 data point");
    }
}

