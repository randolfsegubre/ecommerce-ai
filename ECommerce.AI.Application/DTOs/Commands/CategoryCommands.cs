using FluentValidation;

namespace ECommerce.AI.Application.DTOs.Commands;

public record CreateCategoryCommand
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public Guid? ParentCategoryId { get; init; }
}

public record UpdateCategoryCommand
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public Guid? ParentCategoryId { get; init; }
}

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(200).WithMessage("Category name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Category description is required")
            .MaximumLength(1000).WithMessage("Category description must not exceed 1000 characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }
}

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Category ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(200).WithMessage("Category name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Category description is required")
            .MaximumLength(1000).WithMessage("Category description must not exceed 1000 characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }
}