using FluentValidation;
using IPSDataAcquisition.Application.Features.IMUData.Commands.UploadIMUData;

namespace IPSDataAcquisition.Application.Features.IMUData.Validation;

public class UploadIMUDataCommandValidator : AbstractValidator<UploadIMUDataCommand>
{
    public UploadIMUDataCommandValidator()
    {
        RuleFor(x => x.DataPoints)
            .NotNull().WithMessage("data_points is required")
            .NotEmpty().WithMessage("data_points must contain at least 1 data point");

        RuleForEach(x => x.DataPoints).ChildRules(point =>
        {
            point.RuleFor(p => p.Timestamp)
                .GreaterThan(0).WithMessage("timestamp must be a positive number");
            
            // Note: All sensor fields are nullable - no validation required
            // Devices may not have all sensors, so null values are acceptable
        });
    }
}

