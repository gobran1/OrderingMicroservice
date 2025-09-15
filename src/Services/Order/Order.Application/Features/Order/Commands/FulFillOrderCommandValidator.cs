using FluentValidation;

namespace Order.Application.Features.Order.Commands;

public class FulfillOrderCommandValidator : AbstractValidator<FulfillOrderCommand>
{
    public FulfillOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotNull()
            .NotEmpty()
            .WithMessage("order id is required");
    }
}
