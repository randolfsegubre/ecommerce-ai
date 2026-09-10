using AutoMapper;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Application.DTOs.Commands;
using ECommerce.AI.Domain.Entities;
using ECommerce.AI.Domain.Interfaces;
using ECommerce.AI.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ECommerce.AI.Application.Handlers.Products;

// MediatR requests (the "command" side of CQRS) - one record per write
// operation. MediatR dispatches each to the single handler below that
// implements IRequestHandler<TRequest, TResponse> for it; controllers never
// call these handlers directly, they just send the request via IMediator.
public record CreateProductRequest(DTOs.Commands.CreateProductCommand Command) : IRequest<ProductDto>;
public record UpdateProductRequest(Guid Id, DTOs.Commands.CreateProductCommand Command) : IRequest<ProductDto>;
public record DeleteProductRequest(Guid Id) : IRequest<bool>;

/// <summary>
/// MediatR command handler for creating a new Product. Owns every rule that
/// must hold before a product can exist: the request shape (FluentValidation),
/// its category being real, and its SKU being unique - none of that lives in
/// the controller, which only sends the request.
/// </summary>
public class CreateProductHandler : IRequestHandler<CreateProductRequest, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<DTOs.Commands.CreateProductCommand> _validator;

    public CreateProductHandler(IUnitOfWork unitOfWork, IMapper mapper, IValidator<DTOs.Commands.CreateProductCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ProductDto> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        // STEP 1 of 6 - reject a malformed command before touching the database at all.
        await _validator.ValidateAndThrowAsync(request.Command, cancellationToken);

        // STEP 2 of 6 - a Product must belong to a real Category.
        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.Command.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new ArgumentException($"Category with ID {request.Command.CategoryId} does not exist.");

        // STEP 3 of 6 - SKU is the business-level unique identifier for a
        // Product (separate from its database Id), so it needs its own check.
        var existingProduct = await _unitOfWork.Products.GetBySkuAsync(request.Command.SKU, cancellationToken);
        if (existingProduct != null)
            throw new ArgumentException($"Product with SKU {request.Command.SKU} already exists.");

        // STEP 4 of 6 - build the domain value objects (SKU/Money/Weight/
        // Dimensions) up front, so the Product constructor only ever receives
        // already-valid values, never raw primitives it would have to re-validate.
        var sku = new ProductSKU(request.Command.SKU);
        var price = new Money(request.Command.Price);
        var comparePrice = request.Command.ComparePrice.HasValue ? new Money(request.Command.ComparePrice.Value) : null;
        var weight = new ProductWeight(request.Command.Weight);
        var dimensions = !string.IsNullOrWhiteSpace(request.Command.Dimensions)
            ? ProductDimensions.FromString(request.Command.Dimensions)
            : null;

        var product = new Product(
            request.Command.Name,
            request.Command.Description,
            sku,
            price,
            request.Command.StockQuantity,
            request.Command.MinStockLevel,
            weight,
            request.Command.CategoryId,
            comparePrice,
            dimensions,
            request.Command.Brand,
            request.Command.Model
        );

        // STEP 5 of 6 - attach images/specifications to the in-memory
        // aggregate before it's ever saved, so the whole Product (with its
        // child collections) is inserted as one consistent unit.
        foreach (var imageCommand in request.Command.Images)
        {
            var image = new ProductImage(imageCommand.ImageUrl, product.Id, imageCommand.AltText, imageCommand.SortOrder, imageCommand.IsPrimary);
            product.Images.Add(image);
        }

        foreach (var specCommand in request.Command.Specifications)
        {
            var specification = new ProductSpecification(specCommand.Name, specCommand.Value, product.Id, specCommand.SortOrder);
            product.Specifications.Add(specification);
        }

        // STEP 6 of 6 - persist, then re-read from the database rather than
        // mapping the in-memory `product` directly, so the returned DTO
        // reflects exactly what got stored (defaults, computed columns, etc.).
        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdProduct = await _unitOfWork.Products.GetByIdAsync(product.Id, cancellationToken);
        return _mapper.Map<ProductDto>(createdProduct!);
    }
}

/// <summary>
/// MediatR command handler for updating an existing Product. Re-runs the
/// same category/SKU checks CreateProductHandler does (an update can change
/// both), then applies changes through the Product entity's own DDD methods
/// (UpdateBasicInfo/SetPrice/etc.) rather than setting properties directly -
/// keeps any future invariant enforcement inside the entity, not scattered
/// across every caller that happens to update a Product.
/// </summary>
public class UpdateProductHandler : IRequestHandler<UpdateProductRequest, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<DTOs.Commands.CreateProductCommand> _validator;

    public UpdateProductHandler(IUnitOfWork unitOfWork, IMapper mapper, IValidator<DTOs.Commands.CreateProductCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ProductDto> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        // STEP 1 of 5 - validate the incoming command shape, then confirm the
        // target Product actually exists before checking anything else about it.
        await _validator.ValidateAndThrowAsync(request.Command, cancellationToken);

        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
            throw new ArgumentException($"Product with ID {request.Id} does not exist.");

        // STEP 2 of 5 - same category-exists rule as create.
        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.Command.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new ArgumentException($"Category with ID {request.Command.CategoryId} does not exist.");

        // STEP 3 of 5 - same SKU-uniqueness rule as create, but excluding this
        // Product's own current row (an update that doesn't change the SKU
        // must not reject itself as "already exists").
        var existingProduct = await _unitOfWork.Products.GetBySkuAsync(request.Command.SKU, cancellationToken);
        if (existingProduct != null && existingProduct.Id != request.Id)
            throw new ArgumentException($"Product with SKU {request.Command.SKU} already exists.");

        // STEP 4 of 5 - rebuild the value objects from the incoming command,
        // same reasoning as CreateProductHandler's STEP 4.
        var newSku = new ProductSKU(request.Command.SKU);
        var price = new Money(request.Command.Price);
        var comparePrice = request.Command.ComparePrice.HasValue ? new Money(request.Command.ComparePrice.Value) : null;
        var weight = new ProductWeight(request.Command.Weight);
        var dimensions = !string.IsNullOrWhiteSpace(request.Command.Dimensions)
            ? ProductDimensions.FromString(request.Command.Dimensions)
            : null;

        // STEP 5 of 5 - apply every change through the entity's own methods
        // (not `product.Name = ...`), persist, then re-read and map, same
        // "return what was actually stored" reasoning as create.
        product.UpdateBasicInfo(request.Command.Name, request.Command.Description, request.Command.Brand, request.Command.Model);
        product.UpdateSKU(newSku);
        product.SetPrice(price, comparePrice);
        product.SetStock(request.Command.StockQuantity, request.Command.MinStockLevel);
        product.UpdatePhysicalProperties(weight, dimensions);
        product.SetCategory(request.Command.CategoryId);

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedProduct = await _unitOfWork.Products.GetByIdAsync(product.Id, cancellationToken);
        return _mapper.Map<ProductDto>(updatedProduct!);
    }
}

/// <summary>
/// MediatR command handler for deleting a Product. Deliberately the simplest
/// of the three handlers - no validator, no category/SKU rules apply to a
/// delete - so it stays a plain "find it, remove it, report whether there
/// was anything to remove" operation rather than growing rules it doesn't need.
/// </summary>
public class DeleteProductHandler : IRequestHandler<DeleteProductRequest, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
            return false;

        await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}