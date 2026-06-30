using FluentValidation;

namespace ECommerce.AI.Application.DTOs.Commands;

public record CreateProductCommand
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? ComparePrice { get; init; }
    public int StockQuantity { get; init; }
    public int MinStockLevel { get; init; }
    public double Weight { get; init; }
    public string? Dimensions { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public Guid CategoryId { get; init; }
    public List<CreateProductImageCommand> Images { get; init; } = new();
    public List<CreateProductSpecificationCommand> Specifications { get; init; } = new();
}

public record CreateProductImageCommand
{
    public string ImageUrl { get; init; } = string.Empty;
    public string? AltText { get; init; }
    public int SortOrder { get; init; }
    public bool IsPrimary { get; init; }
}

public record CreateProductSpecificationCommand
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(300).WithMessage("Product name must not exceed 300 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required")
            .MaximumLength(2000).WithMessage("Product description must not exceed 2000 characters");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.ComparePrice)
            .GreaterThan(x => x.Price).WithMessage("Compare price must be greater than price")
            .When(x => x.ComparePrice.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.MinStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Min stock level cannot be negative");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleForEach(x => x.Images).SetValidator(new CreateProductImageCommandValidator());
        RuleForEach(x => x.Specifications).SetValidator(new CreateProductSpecificationCommandValidator());
    }
}

public class CreateProductImageCommandValidator : AbstractValidator<CreateProductImageCommand>
{
    public CreateProductImageCommandValidator()
    {
        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Image URL is required")
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters");

        RuleFor(x => x.AltText)
            .MaximumLength(200).WithMessage("Alt text must not exceed 200 characters");
    }
}

public class CreateProductSpecificationCommandValidator : AbstractValidator<CreateProductSpecificationCommand>
{
    public CreateProductSpecificationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Specification name is required")
            .MaximumLength(100).WithMessage("Specification name must not exceed 100 characters");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Specification value is required")
            .MaximumLength(500).WithMessage("Specification value must not exceed 500 characters");
    }
}