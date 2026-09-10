using AutoMapper;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Domain.Interfaces;
using MediatR;

namespace ECommerce.AI.Application.Handlers.Products;

// MediatR requests (the "query" side of CQRS) - each is a pure read, no
// side effects, dispatched to exactly one handler below the same way the
// command requests in ProductCommandHandlers.cs are.
public record GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>;
public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
public record GetProductBySkuQuery(string Sku) : IRequest<ProductDto?>;
public record GetFeaturedProductsQuery : IRequest<IEnumerable<ProductDto>>;
public record SearchProductsQuery(string SearchTerm) : IRequest<IEnumerable<ProductDto>>;

/// <summary>
/// MediatR query handler returning every Product, with its images eagerly
/// loaded (<see cref="IUnitOfWork.Products"/>'s GetProductsWithImagesAsync,
/// not the plain GetByIdAsync the other handlers use) so a product listing
/// page never has to issue a second request per product just for its photo.
/// </summary>
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

/// <summary>
/// MediatR query handler for looking up a single Product by its database Id.
/// Returns null rather than throwing when nothing matches, so the caller
/// (typically a controller) decides how to turn "not found" into an HTTP
/// response, instead of this handler making that decision for every caller.
/// </summary>
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

/// <summary>
/// MediatR query handler for looking up a single Product by its SKU (the
/// business-level identifier customers/catalogs actually use, as opposed to
/// the internal database Id <see cref="GetProductByIdHandler"/> looks up by).
/// </summary>
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

/// <summary>
/// MediatR query handler for the storefront's "featured products" section.
/// What makes a Product "featured" is decided entirely by the repository
/// (<see cref="IUnitOfWork.Products"/>'s GetFeaturedProductsAsync) - this
/// handler only forwards the request and maps the result, it has no
/// business rules of its own.
/// </summary>
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

/// <summary>
/// MediatR query handler for free-text product search. The actual matching
/// logic (which fields, how fuzzy) lives in the repository's
/// SearchProductsAsync, not here - this handler stays a thin pass-through
/// so the search implementation can change without touching this class.
/// </summary>
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