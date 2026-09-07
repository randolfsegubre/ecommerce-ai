using AutoMapper;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Application.DTOs.Commands;
using ECommerce.AI.Domain.Entities;
using ECommerce.AI.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace ECommerce.AI.Application.Handlers.Categories;

// MediatR Commands
public record CreateCategoryRequest(CreateCategoryCommand Command) : IRequest<CategoryDto>;
public record UpdateCategoryRequest(UpdateCategoryCommand Command) : IRequest<CategoryDto>;
public record DeleteCategoryRequest(Guid Id) : IRequest<bool>;

// Command Handlers
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
        await _validator.ValidateAndThrowAsync(request.Command, cancellationToken);

        if (request.Command.ParentCategoryId.HasValue)
        {
            var parentExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.Command.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                throw new ArgumentException($"Parent category with ID {request.Command.ParentCategoryId} does not exist.");
        }

        var category = _mapper.Map<Category>(request.Command);

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }
}

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
        await _validator.ValidateAndThrowAsync(request.Command, cancellationToken);

        var category = await _unitOfWork.Categories.GetByIdAsync(request.Command.Id, cancellationToken);
        if (category == null)
            throw new ArgumentException($"Category with ID {request.Command.Id} does not exist.");

        if (request.Command.ParentCategoryId.HasValue)
        {
            if (request.Command.ParentCategoryId.Value == category.Id)
                throw new ArgumentException("A category cannot be its own parent.");

            var parentExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.Command.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                throw new ArgumentException($"Parent category with ID {request.Command.ParentCategoryId} does not exist.");
        }

        category.UpdateDetails(request.Command.Name, request.Command.Description, request.Command.ImageUrl);
        category.SetParentCategory(request.Command.ParentCategoryId);

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }
}

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
