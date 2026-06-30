using Microsoft.AspNetCore.Mvc;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Application.DTOs.Commands;

namespace ECommerce.AI.API.Controllers;

/// <summary>
/// Product Images API controller for managing product images
/// </summary>
[ApiController]
[Route("api/products/{productId:guid}/images")]
[Produces("application/json")]
[Tags("Product Images")]
public class ProductImagesController : ControllerBase
{
    private readonly ILogger<ProductImagesController> _logger;

    public ProductImagesController(ILogger<ProductImagesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all images for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of product images</returns>
    /// <response code="200">Returns the list of product images</response>
    /// <response code="404">Product not found</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ProductImageDto>>> GetProductImages(Guid productId)
    {
        if (productId == Guid.Empty)
            return NotFound("Product not found");

        // TODO: Implement with MediatR handler
        var sampleImages = new List<ProductImageDto>
        {
            new ProductImageDto
            {
                Id = Guid.NewGuid(),
                ImageUrl = "https://example.com/images/sample1.jpg",
                AltText = "Sample product image 1",
                SortOrder = 1,
                IsPrimary = true,
                ProductId = productId
            },
            new ProductImageDto
            {
                Id = Guid.NewGuid(),
                ImageUrl = "https://example.com/images/sample2.jpg",
                AltText = "Sample product image 2",
                SortOrder = 2,
                IsPrimary = false,
                ProductId = productId
            }
        };

        return Ok(sampleImages);
    }

    /// <summary>
    /// Get a specific product image
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID</param>
    /// <returns>Product image details</returns>
    /// <response code="200">Returns the product image</response>
    /// <response code="404">Product or image not found</response>
    [HttpGet("{imageId:guid}")]
    [ProducesResponseType(typeof(ProductImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductImageDto>> GetProductImage(Guid productId, Guid imageId)
    {
        if (productId == Guid.Empty || imageId == Guid.Empty)
            return NotFound();

        // TODO: Implement with MediatR handler
        var image = new ProductImageDto
        {
            Id = imageId,
            ImageUrl = "https://example.com/images/sample.jpg",
            AltText = "Sample product image",
            SortOrder = 1,
            IsPrimary = true,
            ProductId = productId
        };

        return Ok(image);
    }

    /// <summary>
    /// Add a new image to a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="command">Image creation data</param>
    /// <returns>Created product image</returns>
    /// <response code="201">Image added successfully</response>
    /// <response code="400">Invalid image data</response>
    /// <response code="404">Product not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductImageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductImageDto>> AddProductImage(Guid productId, [FromBody] CreateProductImageCommand command)
    {
        if (productId == Guid.Empty)
            return NotFound("Product not found");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement with MediatR handler
        var createdImage = new ProductImageDto
        {
            Id = Guid.NewGuid(),
            ImageUrl = command.ImageUrl,
            AltText = command.AltText,
            SortOrder = command.SortOrder,
            IsPrimary = command.IsPrimary,
            ProductId = productId
        };

        return CreatedAtAction(nameof(GetProductImage), 
            new { productId = productId, imageId = createdImage.Id }, 
            createdImage);
    }

    /// <summary>
    /// Update a product image
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID</param>
    /// <param name="command">Image update data</param>
    /// <returns>Updated product image</returns>
    /// <response code="200">Image updated successfully</response>
    /// <response code="400">Invalid image data</response>
    /// <response code="404">Product or image not found</response>
    [HttpPut("{imageId:guid}")]
    [ProducesResponseType(typeof(ProductImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductImageDto>> UpdateProductImage(Guid productId, Guid imageId, [FromBody] UpdateProductImageCommand command)
    {
        if (productId == Guid.Empty || imageId == Guid.Empty)
            return NotFound();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement with MediatR handler
        var updatedImage = new ProductImageDto
        {
            Id = imageId,
            ImageUrl = command.ImageUrl,
            AltText = command.AltText,
            SortOrder = command.SortOrder,
            IsPrimary = command.IsPrimary,
            ProductId = productId
        };

        return Ok(updatedImage);
    }

    /// <summary>
    /// Delete a product image
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Image deleted successfully</response>
    /// <response code="404">Product or image not found</response>
    [HttpDelete("{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProductImage(Guid productId, Guid imageId)
    {
        if (productId == Guid.Empty || imageId == Guid.Empty)
            return NotFound();

        // TODO: Implement with MediatR handler
        return NoContent();
    }

    /// <summary>
    /// Set an image as primary for the product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Primary image set successfully</response>
    /// <response code="404">Product or image not found</response>
    [HttpPost("{imageId:guid}/set-primary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetPrimaryImage(Guid productId, Guid imageId)
    {
        if (productId == Guid.Empty || imageId == Guid.Empty)
            return NotFound();

        // TODO: Implement with MediatR handler
        return NoContent();
    }

    /// <summary>
    /// Upload an image file for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="file">Image file</param>
    /// <param name="altText">Alternative text for the image</param>
    /// <param name="sortOrder">Sort order</param>
    /// <param name="isPrimary">Whether this should be the primary image</param>
    /// <returns>Created product image</returns>
    /// <response code="201">Image uploaded successfully</response>
    /// <response code="400">Invalid file or data</response>
    /// <response code="404">Product not found</response>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ProductImageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductImageDto>> UploadProductImage(
        Guid productId,
        IFormFile file,
        [FromForm] string? altText = null,
        [FromForm] int sortOrder = 0,
        [FromForm] bool isPrimary = false)
    {
        if (productId == Guid.Empty)
            return NotFound("Product not found");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed.");

        // Validate file size (5MB limit)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("File size too large. Maximum size is 5MB.");

        // TODO: Implement file upload to cloud storage (Azure Blob, AWS S3, etc.)
        var imageUrl = $"https://example.com/images/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var createdImage = new ProductImageDto
        {
            Id = Guid.NewGuid(),
            ImageUrl = imageUrl,
            AltText = altText,
            SortOrder = sortOrder,
            IsPrimary = isPrimary,
            ProductId = productId
        };

        return CreatedAtAction(nameof(GetProductImage), 
            new { productId = productId, imageId = createdImage.Id }, 
            createdImage);
    }
}

/// <summary>
/// Update product image command for PUT operations
/// </summary>
public class UpdateProductImageCommand
{
    /// <summary>
    /// Image URL
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Alternative text for accessibility
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Sort order for display
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Whether this is the primary image
    /// </summary>
    public bool IsPrimary { get; set; } = false;
}