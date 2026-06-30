using AutoMapper;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Domain.Interfaces;
using MediatR;

namespace ECommerce.AI.Application.Handlers.Products;

// Queries
public record GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>;
public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
public record GetProductBySkuQuery(string Sku) : IRequest<ProductDto?>;
public record GetFeaturedProductsQuery : IRequest<IEnumerable<ProductDto>>;
public record SearchProductsQuery(string SearchTerm) : IRequest<IEnumerable<ProductDto>>;

// Query Handlers
public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllProductsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetProductsWithImagesAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }
}

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }
}

public class GetProductBySkuHandler : IRequestHandler<GetProductBySkuQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductBySkuHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductBySkuQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetBySkuAsync(request.Sku, cancellationToken);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }
}

public class GetFeaturedProductsHandler : IRequestHandler<GetFeaturedProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetFeaturedProductsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetFeaturedProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetFeaturedProductsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }
}

public class SearchProductsHandler : IRequestHandler<SearchProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchProductsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.SearchProductsAsync(request.SearchTerm, cancellationToken);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }
}