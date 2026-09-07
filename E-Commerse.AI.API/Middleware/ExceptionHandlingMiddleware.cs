using System.Net;
using FluentValidation;

namespace ECommerce.AI.API.Middleware;

/// <summary>
/// Translates exceptions thrown by MediatR handlers into proper HTTP problem responses instead of
/// letting them bubble up as raw 500s with a stack trace.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for {Path}", context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Validation failed", new
            {
                errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for {Path}", context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for {Path}", context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string title, object? extra = null)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["status"] = (int)statusCode
        };

        if (extra is not null)
        {
            foreach (var prop in extra.GetType().GetProperties())
            {
                problem[prop.Name] = prop.GetValue(extra);
            }
        }

        await context.Response.WriteAsJsonAsync(problem);
    }
}
