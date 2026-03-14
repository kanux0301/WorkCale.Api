using Xunit;
using FluentAssertions;
using FluentValidation;
using MediatR;
using WorkCale.Application.Behaviors;

namespace WorkCale.Application.Tests.Common;

public class ValidationBehaviorTests
{
    private record TestRequest(string Value) : IRequest<string>;

    private class AlwaysValidValidator : AbstractValidator<TestRequest>
    {
        public AlwaysValidValidator()
        {
            RuleFor(r => r.Value).NotEmpty();
        }
    }

    private class AlwaysFailValidator : AbstractValidator<TestRequest>
    {
        public AlwaysFailValidator()
        {
            RuleFor(r => r.Value).Must(_ => false).WithMessage("Always fails");
        }
    }

    private static RequestHandlerDelegate<string> NextReturning(string value) =>
        _ => Task.FromResult(value);

    [Fact]
    public async Task Handle_CallsNext_WhenNoValidators()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);

        var result = await behavior.Handle(new TestRequest("any"), NextReturning("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_CallsNext_WhenValidationPasses()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new AlwaysValidValidator()]);

        var result = await behavior.Handle(new TestRequest("value"), NextReturning("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ThrowsValidationException_WhenValidationFails()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new AlwaysFailValidator()]);

        var act = async () => await behavior.Handle(
            new TestRequest("any"), NextReturning("ok"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Always fails*");
    }

    [Fact]
    public async Task Handle_ThrowsValidationException_WhenRequestValueIsEmpty()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([new AlwaysValidValidator()]);

        var act = async () => await behavior.Handle(
            new TestRequest(""), NextReturning("ok"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_CollectsAllFailures_FromMultipleValidators()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(
            [new AlwaysFailValidator(), new AlwaysFailValidator()]);

        var act = async () => await behavior.Handle(
            new TestRequest("x"), NextReturning("ok"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
