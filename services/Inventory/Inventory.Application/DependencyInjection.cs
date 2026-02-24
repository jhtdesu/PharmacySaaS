using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Đăng ký MediatR và quét tất cả Handler trong Assembly này
        services.AddMediatR(cf => 
            cf.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}