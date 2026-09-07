using System.Reflection;
using ECommerce.AI.API.Middleware;
using ECommerce.AI.Application;
using ECommerce.AI.Infrastructure;
using ECommerce.AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to ensure we see output
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllers();

// Application (MediatR, AutoMapper, FluentValidation) and Infrastructure (EF Core, repositories)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "E-Commerce AI API",
        Version = "v1",
        Description = "A comprehensive e-commerce API built with Domain-Driven Design principles",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "E-Commerce AI Team",
            Email = "support@ecommerce-ai.com"
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add security definitions for future authentication
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Group endpoints by tags
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((name, api) => true);
});

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Get logger for startup messages
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Apply pending EF Core migrations and seed sample data for local development.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbInitializer.SeedAsync(dbContext);
}

// Log startup information
logger.LogInformation("=== E-Commerce AI API Starting ===");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Content Root: {ContentRoot}", app.Environment.ContentRootPath);

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    logger.LogInformation("Configuring Development environment...");
    
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce AI API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root (/)
        c.DocumentTitle = "E-Commerce AI API Documentation";
        c.DefaultModelsExpandDepth(-1); // Disable swagger schemas section
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
    });
    
    app.UseCors("DevelopmentCors");
    logger.LogInformation("Swagger UI configured at root path (/)");
}
else
{
    // In production, you might want to enable Swagger with authentication
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce AI API V1");
        c.RoutePrefix = "api-docs"; // Different route for production
    });
    logger.LogInformation("Production environment configured");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

logger.LogInformation("=== E-Commerce AI API Configuration Complete ===");

app.Run();
