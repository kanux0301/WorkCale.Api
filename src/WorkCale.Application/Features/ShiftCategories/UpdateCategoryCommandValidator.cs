using FluentValidation;
using WorkCale.Application.Common;

namespace WorkCale.Application.Features.ShiftCategories;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Color).HexColor();
        RuleFor(x => x.DefaultStartTime).HHmmFormat("DefaultStartTime")
            .When(x => x.DefaultStartTime != null);
        RuleFor(x => x.DefaultEndTime).HHmmFormat("DefaultEndTime")
            .When(x => x.DefaultEndTime != null);
    }
}
