using Microsoft.AspNetCore.Mvc;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Application.DTOs.Commands;

namespace ECommerce.AI.API.Controllers;

/// <summary>
/// Categories API controller for managing product categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Categories")]
public class CategoriesController : ControllerBase
{
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ILogger<CategoriesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    /// <returns>List of categories</returns>
    /// <response code="200">Returns the list of categories</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        // TODO: Implement with MediatR handler
        var sampleCategories = new List<CategoryDto>
        {
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                IsActive = true,
                ParentCategoryId = null,
                SubCategories = new List<CategoryDto>(),
                Products = new List<ProductDto>(),
                CreatedAt = DateTime.UtcNow
            },
            new CategoryDto
            {
                Id = Guid.NewGuid(),
                Name = "Clothing",
                Description = "Clothing and apparel",
                IsActive = true,
                ParentCategoryId = null,
                SubCategories = new List<CategoryDto>(),
                Products = new List<ProductDto>(),
                CreatedAt = DateTime.UtcNow
            }
        };

        return Ok(sampleCategories);
    }

    /// <summary>
    /// Get a category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details</returns>
    /// <response code="200">Returns the category</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
    {
        // TODO: Implement with MediatR handler
        if (id == Guid.Empty)
            return NotFound();

        var category = new CategoryDto
        {
            Id = id,
            Name = "Electronics",
            Description = "Electronic devices and accessories",
            IsActive = true,
            ParentCategoryId = null,
            SubCategories = new List<CategoryDto>(),
            Products = new List<ProductDto>(),
            CreatedAt = DateTime.UtcNow
        };

        return Ok(category);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="command">Category creation data</param>
    /// <returns>Created category</returns>
    /// <response code="201">Category created successfully</response>
    /// <response code="400">Invalid category data</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // TODO: Implement with MediatR handler
        var createdCategory = new CategoryDto
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            ImageUrl = command.ImageUrl,
            IsActive = true,
            ParentCategoryId = command.ParentCategoryId,
            SubCategories = new List<CategoryDto>(),
            Products = new List<ProductDto>(),
            CreatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, createdCategory);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="command">Category update data</param>
    /// <returns>Updated category</returns>
    /// <response code="200">Category updated successfully</response>
    /// <response code="404">Category not found</response>
    /// <response code="400">Invalid category data</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id == Guid.Empty)
            return NotFound();

        // TODO: Implement with MediatR handler
        var updatedCategory = new CategoryDto
        {
            Id = id,
            Name = command.Name,
            Description = command.Description,
            ImageUrl = command.ImageUrl,
            IsActive = command.IsActive,
            ParentCategoryId = command.ParentCategoryId,
            SubCategories = new List<CategoryDto>(),
            Products = new List<ProductDto>(),
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(updatedCategory);
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Category deleted successfully</response>
    /// <response code="404">Category not found</response>
    /// <response code="400">Category cannot be deleted (has subcategories or products)</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteCategory(Guid id)
    {
        if (id == Guid.Empty)
            return NotFound();

        // TODO: Implement with MediatR handler
        // Check if category has subcategories or products before deletion
        return NoContent();
    }

    /// <summary>
    /// Get root categories (categories without parent)
    /// </summary>
    /// <returns>List of root categories</returns>
    /// <response code="200">Returns the list of root categories</response>
    [HttpGet("roots")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetRootCategories()
    {
        // TODO: Implement with MediatR handler
        var categories = new List<CategoryDto>();
        return Ok(categories);
    }

    /// <summary>
    /// Get subcategories of a category
    /// </summary>
    /// <param name="id">Parent category ID</param>
    /// <returns>List of subcategories</returns>
    /// <response code="200">Returns the list of subcategories</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id:guid}/subcategories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetSubcategories(Guid id)
    {
        if (id == Guid.Empty)
            return NotFound();

        // TODO: Implement with MediatR handler
        var subcategories = new List<CategoryDto>();
        return Ok(subcategories);
    }

    /// <summary>
    /// Get category hierarchy (category with all ancestors and descendants)
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category hierarchy</returns>
    /// <response code="200">Returns the category hierarchy</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id:guid}/hierarchy")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategoryHierarchy(Guid id)
    {
        if (id == Guid.Empty)
            return NotFound();

        // TODO: Implement with MediatR handler
        var categoryHierarchy = new CategoryDto
        {
            Id = id,
            Name = "Sample Category",
            Description = "Sample category with hierarchy",
            IsActive = true,
            SubCategories = new List<CategoryDto>(),
            Products = new List<ProductDto>(),
            CreatedAt = DateTime.UtcNow
        };

        return Ok(categoryHierarchy);
    }
}

/// <summary>
/// Update category command for PUT operations
/// </summary>
public class UpdateCategoryCommand
{
    /// <summary>
    /// Category name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category image URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Parent category ID
    /// </summary>
    public Guid? ParentCategoryId { get; set; }
}