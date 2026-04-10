using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shared.Contracts.Configuration;

namespace Inventory.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        var dotenv = FindFileInCurrentOrParents(root, ".env");

        if (!string.IsNullOrEmpty(dotenv))
            DotEnv.Load(dotenv);

        var config = new ConfigurationBuilder()
            .SetBasePath(root)
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
        
        if (!string.IsNullOrEmpty(pgUser))
            connectionString = connectionString.Replace("PGUSER", pgUser);
        if (!string.IsNullOrEmpty(pgPassword))
            connectionString = connectionString.Replace("PGPASSWORD", pgPassword);

        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new InventoryDbContext(optionsBuilder.Options);
    }

    private static string? FindFileInCurrentOrParents(string startDirectory, string fileName)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, fileName);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        return null;
    }
}
