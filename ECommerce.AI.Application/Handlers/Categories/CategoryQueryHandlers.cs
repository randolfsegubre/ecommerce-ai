using AutoMapper;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Domain.Interfaces;
using MediatR;

namespace ECommerce.AI.Application.Handlers.Categories;

// MediatR requests (the "query" side of CQRS) - pure reads, no side effects,
// same pattern as ProductQueryHandlers.cs's requests.
public record GetAllCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;
public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;
public record GetRootCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;
public record GetActiveCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;
public record GetCategoryWithProductsQuery(Guid Id) : IRequest<CategoryDto?>;

/// <summary>MediatR query handler returning every Category, active or not.</summary>
public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCategoriesHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }
}

/// <summary>MediatR query handler for looking up a single Category by its database Id.</summary>
public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCategoryByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken);
        return category == null ? null : _mapper.Map<CategoryDto>(category);
    }
}

/// <summary>
/// MediatR query handler for the top-level categories only (those with no
/// ParentCategoryId) - what a category navigation menu/breadcrumb root
/// would render, as opposed to <see cref="GetAllCategoriesHandler"/>'s flat
/// list of every category regardless of nesting depth.
/// </summary>
public class GetRootCategoriesHandler : IRequestHandler<GetRootCategoriesQuery, IEnumerable<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRootCategoriesHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(GetRootCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetRootCategoriesAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }
}

/// <summary>
/// MediatR query handler for Categories flagged active - the storefront's
/// "categories customers can currently browse" list, distinct from
/// <see cref="GetAllCategoriesHandler"/> which includes inactive ones too
/// (e.g. a discontinued line an admin hasn't deleted, just hidden).
/// </summary>
public class GetActiveCategoriesHandler : IRequestHandler<GetActiveCategoriesQuery, IEnumerable<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetActiveCategoriesHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetActiveCategoriesAsync(cancellationToken);
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }
}

/// <summary>
/// MediatR query handler for a single Category page that also needs its
/// Products eagerly loaded (<see cref="IUnitOfWork.Categories"/>'s
/// GetWithProductsAsync, not the plain GetByIdAsync
/// <see cref="GetCategoryByIdHandler"/> uses) - avoids an N+1 query when a
/// category-detail page renders both the category and its product grid.
/// </summary>
public class GetCategoryWithProductsHandler : IRequestHandler<GetCategoryWithProductsQuery, CategoryDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCategoryWithProductsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CategoryDto?> Handle(GetCategoryWithProductsQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetWithProductsAsync(request.Id, cancellationToken);
        return category == null ? null : _mapper.Map<CategoryDto>(category);
    }
}
