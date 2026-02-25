using Inventory.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));

        // 2. Đăng ký Interface (Cầu nối giữa Application và Infrastructure)
        // Lưu ý: Sử dụng IInventoryDbContext thay vì IInventoryDbContext như bạn đã đổi tên
        services.AddScoped<IInventoryDbContext>(provider => 
            provider.GetRequiredService<InventoryDbContext>());

        return services;
    }
}