using AutoMapper;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Domain.Interfaces;
using MediatR;

namespace ECommerce.AI.Application.Handlers.Categories;

// Queries
public record GetAllCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;
public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;
public record GetRootCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;
public record GetActiveCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;
public record GetCategoryWithProductsQuery(Guid Id) : IRequest<CategoryDto?>;

// Query Handlers
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
