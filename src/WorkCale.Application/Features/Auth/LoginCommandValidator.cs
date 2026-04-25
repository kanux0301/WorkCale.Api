using FluentValidation;
using WorkCale.Application.Common;

namespace WorkCale.Application.Features.Auth;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).NotEmpty();
    }
}
