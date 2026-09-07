using MediatR;
using Microsoft.AspNetCore.Mvc;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Application.DTOs.Commands;
using ECommerce.AI.Application.Handlers.Categories;

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
    private readonly IMediator _mediator;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger)
    {
        _mediator = mediator;
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
        var categories = await _mediator.Send(new GetAllCategoriesQuery());
        return Ok(categories);
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
        var category = await _mediator.Send(new GetCategoryByIdQuery(id));
        return category is null ? NotFound() : Ok(category);
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
        var createdCategory = await _mediator.Send(new CreateCategoryRequest(command));
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
        if (id != command.Id)
            return BadRequest("Route ID and command ID must match.");

        var updatedCategory = await _mediator.Send(new UpdateCategoryRequest(command));
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
        var deleted = await _mediator.Send(new DeleteCategoryRequest(id));
        return deleted ? NoContent() : NotFound();
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
        var categories = await _mediator.Send(new GetRootCategoriesQuery());
        return Ok(categories);
    }

    /// <summary>
    /// Get a category together with its products
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category with its products loaded</returns>
    /// <response code="200">Returns the category with products</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id:guid}/products")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategoryWithProducts(Guid id)
    {
        var category = await _mediator.Send(new GetCategoryWithProductsQuery(id));
        return category is null ? NotFound() : Ok(category);
    }
}
