using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Configurations;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("Medicines");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
        builder.Property(m => m.SKU).IsRequired().HasMaxLength(50);
        builder.HasIndex(m => m.SKU).IsUnique();
        builder.HasMany(m => m.Batches)
            .WithOne(b => b.Medicine)
            .HasForeignKey(b => b.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}