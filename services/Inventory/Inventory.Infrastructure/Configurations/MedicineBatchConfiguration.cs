using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Configurations;

public class MedicineBatchConfiguration : IEntityTypeConfiguration<MedicineBatch>
{
    public void Configure(EntityTypeBuilder<MedicineBatch> builder)
    {
        builder.ToTable("MedicineBatches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BatchNumber)
            .IsRequired()
            .HasMaxLength(100);

        // Ràng buộc số lượng không được âm (Postgres Check Constraint)
        builder.ToTable(t => t.HasCheckConstraint("CK_Batch_Quantity_NotNegative", "\"CurrentQuantity\" >= 0"));

        builder.Property(b => b.PurchasePrice)
            .HasPrecision(18, 2); // Định dạng tiền tệ
    }
}