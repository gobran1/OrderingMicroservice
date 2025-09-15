using Contract.SharedDTOs;
using FluentValidation;
using Order.Application.Features.Order.DTOs;
using SharedKernel.ValueObjects;

namespace Order.Application.Features.Order.Commands;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CreateOrderDto)
            .NotNull()
            .WithMessage("Order data is required")
            .SetValidator(new CreateOrderDTOValidator());
    }
}

public class CreateOrderDTOValidator : AbstractValidator<CreateOrderDTO>
{
    public CreateOrderDTOValidator()
    {
        RuleFor(x => x.User)
            .NotNull()
            .WithMessage("User information is required")
            .SetValidator(new CreateUserVoDTOValidator());

        RuleFor(x => x.DeliveryAddress)
            .SetValidator(new CreateAddressDTOValidator())
            .When(x => x.DeliveryAddress != null);
        
        RuleFor(x => x.OrderItems)
            .NotNull()
            .WithMessage("Order items are required")
            .NotEmpty()
            .WithMessage("Order must contain at least one item");
        
        RuleForEach(x => x.OrderItems)
            .SetValidator(new CreateOrderItemDTOValidator());
    }
}

public class CreateUserVoDTOValidator : AbstractValidator<CreateUserVoDTO>
{
    public CreateUserVoDTOValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("User name is required")
            .MaximumLength(100)
            .WithMessage("User name cannot exceed 100 characters");

        RuleFor(x => x.UserEmail)
            .NotEmpty()
            .WithMessage("User email is required")
            .EmailAddress()
            .WithMessage("Valid email address is required")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .When(x => x.UserId.HasValue)
            .WithMessage("User ID cannot be empty GUID");
    }
}

public class CreateAddressDTOValidator : AbstractValidator<CreateAddressDTO?>
{
    public CreateAddressDTOValidator()
    {
        RuleFor(x => x.Address1)
            .NotEmpty()
            .WithMessage("Address line 1 is required")
            .MaximumLength(200)
            .WithMessage("Address line 1 cannot exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State/Province is required")
            .MaximumLength(100)
            .WithMessage("State cannot exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(100)
            .WithMessage("Country cannot exceed 100 characters");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .WithMessage("Zip/Postal code is required")
            .MaximumLength(20)
            .WithMessage("Zip code cannot exceed 20 characters");

        RuleFor(x => x.Address2)
            .MaximumLength(200)
            .WithMessage("Address line 2 cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Address2));
    }
}

public class CreateOrderItemDTOValidator : AbstractValidator<CreateOrderItemDTO>
{
    public CreateOrderItemDTOValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Product ID cannot be empty");

        RuleFor(x => x.Quantity)
            .NotNull()
            .WithMessage("Quantity is required")
            .SetValidator(new CreateQuantityDTOValidator());
    }
}

public class CreateQuantityDTOValidator : AbstractValidator<CreateQuantityDTO>
{
    public CreateQuantityDTOValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Quantity cannot exceed 1000 per item");

        RuleFor(x => x.Uom)
            .NotNull()
            .WithMessage("Uom is required");
    }
}