using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure;

public class InventoryDbContext : DbContext, IInventoryDbContext
{
    private readonly ITenantService? _tenantService;
    

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options, ITenantService tenantService) 
        : base(options)
    {
        _tenantService = tenantService;
    }

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public Guid CurrentTenantId => _tenantService?.GetCurrentTenantId() ?? Guid.Empty;

    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicineBatch> Batches => Set<MedicineBatch>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);

        modelBuilder.Entity<InventoryTransaction>().HasQueryFilter(t => t.TenantId == CurrentTenantId && !t.IsDeleted);
        modelBuilder.Entity<Sale>().HasQueryFilter(s => s.TenantId == CurrentTenantId && !s.IsDeleted);
        modelBuilder.Entity<SaleItem>().HasQueryFilter(si => si.TenantId == CurrentTenantId && !si.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService?.GetCurrentTenantId();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseData baseEntity && entry.State == EntityState.Added)
            {
                baseEntity.TenantId = tenantId.GetValueOrDefault();
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}