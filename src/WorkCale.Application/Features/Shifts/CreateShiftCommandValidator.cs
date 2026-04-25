using FluentValidation;
using WorkCale.Application.Common;

namespace WorkCale.Application.Features.Shifts;

public class CreateShiftCommandValidator : AbstractValidator<CreateShiftCommand>
{
    public CreateShiftCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty().HHmmFormat("StartTime");
        RuleFor(x => x.EndTime).NotEmpty().HHmmFormat("EndTime");
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
