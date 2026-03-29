using Microsoft.Extensions.DependencyInjection;

namespace Shared.Contracts.ExceptionHandling;

public static class ExceptionHandlingServiceCollectionExtensions
{
    public static IServiceCollection AddSharedExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
