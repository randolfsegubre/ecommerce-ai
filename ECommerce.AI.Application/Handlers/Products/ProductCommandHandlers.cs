using AutoMapper;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Application.DTOs.Commands;
using ECommerce.AI.Domain.Entities;
using ECommerce.AI.Domain.Interfaces;
using ECommerce.AI.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ECommerce.AI.Application.Handlers.Products;

// MediatR Commands - using different names to avoid conflicts
public record CreateProductRequest(DTOs.Commands.CreateProductCommand Command) : IRequest<ProductDto>;
public record UpdateProductRequest(Guid Id, DTOs.Commands.CreateProductCommand Command) : IRequest<ProductDto>;
public record DeleteProductRequest(Guid Id) : IRequest<bool>;

// Command Handlers
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
        await _validator.ValidateAndThrowAsync(request.Command, cancellationToken);

        // Check if category exists
        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.Command.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new ArgumentException($"Category with ID {request.Command.CategoryId} does not exist.");

        // Check if SKU already exists
        var existingProduct = await _unitOfWork.Products.GetBySkuAsync(request.Command.SKU, cancellationToken);
        if (existingProduct != null)
            throw new ArgumentException($"Product with SKU {request.Command.SKU} already exists.");

        // Create product with value objects
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
        
        // Add images to the product's collection
        foreach (var imageCommand in request.Command.Images)
        {
            var image = new ProductImage(imageCommand.ImageUrl, product.Id, imageCommand.AltText, imageCommand.SortOrder, imageCommand.IsPrimary);
            product.Images.Add(image);
        }

        // Add specifications to the product's collection
        foreach (var specCommand in request.Command.Specifications)
        {
            var specification = new ProductSpecification(specCommand.Name, specCommand.Value, product.Id, specCommand.SortOrder);
            product.Specifications.Add(specification);
        }

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdProduct = await _unitOfWork.Products.GetByIdAsync(product.Id, cancellationToken);
        return _mapper.Map<ProductDto>(createdProduct!);
    }
}

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
        await _validator.ValidateAndThrowAsync(request.Command, cancellationToken);

        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
            throw new ArgumentException($"Product with ID {request.Id} does not exist.");

        // Check if category exists
        var categoryExists = await _unitOfWork.Categories.AnyAsync(c => c.Id == request.Command.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new ArgumentException($"Category with ID {request.Command.CategoryId} does not exist.");

        // Check if SKU already exists (excluding current product)
        var existingProduct = await _unitOfWork.Products.GetBySkuAsync(request.Command.SKU, cancellationToken);
        if (existingProduct != null && existingProduct.Id != request.Id)
            throw new ArgumentException($"Product with SKU {request.Command.SKU} already exists.");

        // Create value objects
        var newSku = new ProductSKU(request.Command.SKU);
        var price = new Money(request.Command.Price);
        var comparePrice = request.Command.ComparePrice.HasValue ? new Money(request.Command.ComparePrice.Value) : null;
        var weight = new ProductWeight(request.Command.Weight);
        var dimensions = !string.IsNullOrWhiteSpace(request.Command.Dimensions) 
            ? ProductDimensions.FromString(request.Command.Dimensions) 
            : null;

        // Update product properties using DDD methods
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