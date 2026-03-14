using FluentValidation;

namespace WorkCale.Application.Features.Shifts;

public class CreateShiftCommandValidator : AbstractValidator<CreateShiftCommand>
{
    public CreateShiftCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.StartTime)
            .NotEmpty()
            .Matches(@"^\d{2}:\d{2}$").WithMessage("StartTime must be in HH:mm format.");
        RuleFor(x => x.EndTime)
            .NotEmpty()
            .Matches(@"^\d{2}:\d{2}$").WithMessage("EndTime must be in HH:mm format.");
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
