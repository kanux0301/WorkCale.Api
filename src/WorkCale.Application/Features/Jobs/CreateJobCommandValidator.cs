using FluentValidation;
using WorkCale.Application.Common;

namespace WorkCale.Application.Features.Jobs;

public class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Color).HexColor();
        RuleFor(x => x.Icon).MaximumLength(50);
    }
}
