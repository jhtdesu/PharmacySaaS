using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Contracts.Configuration;

namespace Auth.Api.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        // Load .env file from parent directory
        var root = Directory.GetCurrentDirectory();
        var parent = Directory.GetParent(root)?.Parent?.Parent; // Go up to services directory
        var dotenv = parent != null ? Path.Combine(parent.FullName, ".env") : "";
        
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

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AuthDbContext(optionsBuilder.Options);
    }
}
