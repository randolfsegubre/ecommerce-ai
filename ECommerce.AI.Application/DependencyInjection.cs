using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.AI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
