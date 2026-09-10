using AutoMapper;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Application.DTOs.Commands;
using ECommerce.AI.Domain.Entities;
using ECommerce.AI.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace ECommerce.AI.Application.Handlers.Categories;

// MediatR requests (the "command" side of CQRS) - same pattern as
// ProductCommandHandlers.cs's requests, one record per write operation.
public record CreateCategoryRequest(CreateCategoryCommand Command) : IRequest<CategoryDto>;
public record UpdateCategoryRequest(UpdateCategoryCommand Command) : IRequest<CategoryDto>;
public record DeleteCategoryRequest(Guid Id) : IRequest<bool>;

/// <summary>
/// MediatR command handler for creating a Category. Categories can nest
/// (ParentCategoryId), so the one real rule here is that a declared parent
/// must actually exist - everything else about the new Category comes
/// straight from the validated command via AutoMapper.
/// </summary>
public class CreateCategoryHandler : IRequestHandler<CreateCategoryRequest, CategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCategoryCommand> _validator;

    public CreateCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateCategoryCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<CategoryDto> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        // STEP 1 of 3 - reject a malformed command before checking anything else.
        await _validator.ValidateAndThrowAsync(request.Command, cancellationToken);

        // STEP 2 of 3 - a declared parent category must be real; ParentCategoryId
        // is optional (a top-level category has none), so this only runs when set.
        if (request.Command.ParentCategoryId.HasValue)
        {
            var parentExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.Command.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                throw new ArgumentException($"Parent category with ID {request.Command.ParentCategoryId} does not exist.");
        }

        // STEP 3 of 3 - AutoMapper builds the entity directly from the command
        // (no value objects to construct here, unlike Product), then persist.
        var category = _mapper.Map<Category>(request.Command);

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }
}

/// <summary>
/// MediatR command handler for updating a Category. Beyond CreateCategoryHandler's
/// parent-exists check, an update has one extra rule an update-only operation
/// can even trigger: a category being re-pointed to itself as its own parent.
/// </summary>
public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryRequest, CategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateCategoryCommand> _validator;

    public UpdateCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper, IValidator<UpdateCategoryCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        // STEP 1 of 3 - validate the command, then confirm the target Category exists.
        await _validator.ValidateAndThrowAsync(request.Command, cancellationToken);

        var category = await _unitOfWork.Categories.GetByIdAsync(request.Command.Id, cancellationToken);
        if (category == null)
            throw new ArgumentException($"Category with ID {request.Command.Id} does not exist.");

        // STEP 2 of 3 - self-parenting would create a category that is its own
        // ancestor; checked before the generic parent-exists check below,
        // since "does X exist" would trivially pass for X's own Id.
        if (request.Command.ParentCategoryId.HasValue)
        {
            if (request.Command.ParentCategoryId.Value == category.Id)
                throw new ArgumentException("A category cannot be its own parent.");

            var parentExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.Command.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                throw new ArgumentException($"Parent category with ID {request.Command.ParentCategoryId} does not exist.");
        }

        // STEP 3 of 3 - apply changes through the entity's own methods (same
        // "never set properties directly" reasoning as UpdateProductHandler), then persist.
        category.UpdateDetails(request.Command.Name, request.Command.Description, request.Command.ImageUrl);
        category.SetParentCategory(request.Command.ParentCategoryId);

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }
}

/// <summary>
/// MediatR command handler for deleting a Category. The one real rule -
/// <see cref="Category.CanBeDeleted"/> - refuses to delete a Category that
/// still has subcategories or products hanging off it, so deleting one
/// never silently orphans data that pointed at it.
/// </summary>
public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryRequest, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
            return false;

        if (!category.CanBeDeleted())
            throw new InvalidOperationException("Category cannot be deleted while it has subcategories or products.");

        await _unitOfWork.Categories.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
