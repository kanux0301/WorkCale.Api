using FluentValidation;

namespace WorkCale.Application.Features.CalendarShares;

public class GrantCalendarShareCommandValidator : AbstractValidator<GrantCalendarShareCommand>
{
    public GrantCalendarShareCommandValidator()
    {
        RuleFor(x => x.ViewerUserId).NotEmpty();
        RuleFor(x => x).Must(x => x.OwnerUserId != x.ViewerUserId)
            .WithMessage("You cannot share your calendar with yourself.");
    }
}
