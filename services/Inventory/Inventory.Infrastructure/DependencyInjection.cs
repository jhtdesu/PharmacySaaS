using Inventory.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        connectionString = connectionString
            .Replace("PGUSER", Environment.GetEnvironmentVariable("PGUSER") ?? "")
            .Replace("PGPASSWORD", Environment.GetEnvironmentVariable("PGPASSWORD") ?? "");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));
                
        services.AddScoped<IInventoryDbContext>(provider => 
            provider.GetRequiredService<InventoryDbContext>());

        return services;
    }
}