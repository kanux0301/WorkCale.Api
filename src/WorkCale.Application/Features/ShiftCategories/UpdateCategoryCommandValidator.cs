using FluentValidation;

namespace WorkCale.Application.Features.ShiftCategories;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Color)
            .NotEmpty()
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be a valid hex color (e.g. #F59E0B).");
        RuleFor(x => x.DefaultStartTime)
            .Matches(@"^\d{2}:\d{2}$").WithMessage("DefaultStartTime must be in HH:mm format.")
            .When(x => x.DefaultStartTime != null);
        RuleFor(x => x.DefaultEndTime)
            .Matches(@"^\d{2}:\d{2}$").WithMessage("DefaultEndTime must be in HH:mm format.")
            .When(x => x.DefaultEndTime != null);
    }
}
