using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure;

public class InventoryDbContext : DbContext, IInventoryDbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicineBatch> Batches => Set<MedicineBatch>();

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
}