using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Common.Interfaces;

public interface IInventoryDbContext
{
    DbSet<Medicine> Medicines { get; }
    DbSet<MedicineBatch> Batches { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}