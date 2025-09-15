using FluentValidation;

namespace Order.Application.Features.Order.Commands;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotNull()
            .NotEmpty()
            .WithMessage("order id is required");
    }
}
