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

    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicineBatch> Batches => Set<MedicineBatch>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);

        modelBuilder.Entity<InventoryTransaction>().HasQueryFilter(t => 
        !t.IsDeleted);

        modelBuilder.Entity<InventoryTransaction>()
        .HasOne(t => t.MedicineBatch)
        .WithMany()
        .HasForeignKey(t => t.MedicineBatchId)
        .OnDelete(DeleteBehavior.Restrict);
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