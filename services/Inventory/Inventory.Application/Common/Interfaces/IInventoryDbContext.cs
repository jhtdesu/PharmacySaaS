using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Inventory.Application.Common.Interfaces;

public interface IInventoryDbContext
{
    DatabaseFacade Database { get; }
    DbSet<Medicine> Medicines { get; }
    DbSet<MedicineBatch> Batches { get; }
    DbSet<Sale> Sales { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}