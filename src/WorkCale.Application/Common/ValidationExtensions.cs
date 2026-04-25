using FluentValidation;

namespace WorkCale.Application.Common;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(255);

    public static IRuleBuilderOptions<T, string?> HHmmFormat<T>(this IRuleBuilder<T, string?> rule, string fieldName) =>
        rule.Matches(TimeFormats.HHmmRegex).WithMessage($"{fieldName} {TimeFormats.HHmmMessage}");

    public static IRuleBuilderOptions<T, string> HexColor<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be a valid hex color (e.g. #F59E0B).");
}
