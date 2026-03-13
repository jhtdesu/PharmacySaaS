using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransaction");
        builder
        .HasOne(t => t.MedicineBatch)
        .WithMany()
        .HasForeignKey(t => t.MedicineBatchId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}