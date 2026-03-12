using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Medicines.Dispense;

public class DispenseMedicineCommandHandler : IRequestHandler<DispenseMedicineCommand>
{
    private readonly IInventoryDbContext _context; 

    public DispenseMedicineCommandHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DispenseMedicineCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        var availableBatches = await _context.Batches
            .Where(b => b.MedicineId == request.MedicineId && b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow)
            .OrderBy(b => b.ExpiryDate).ToListAsync(cancellationToken);
            
        var totalAvailable = availableBatches.Sum(b => b.CurrentQuantity);
        if (totalAvailable < request.Quantity)
        {
            throw new InvalidOperationException($"Insufficient stock. Requested: {request.Quantity}, Available: {totalAvailable}");
        }

        var remainingToDispense = request.Quantity;

        foreach (var batch in availableBatches)
        {
            if (remainingToDispense == 0) break;

            int quantityDeducted = 0;
            if (batch.CurrentQuantity >= remainingToDispense)
            {
                quantityDeducted = remainingToDispense;
                batch.CurrentQuantity -= remainingToDispense;
                remainingToDispense = 0;
            }
            else
            {
                quantityDeducted = batch.CurrentQuantity;
                remainingToDispense -= batch.CurrentQuantity;
                batch.CurrentQuantity = 0;
            }

            var transaction = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                MedicineId = request.MedicineId,
                MedicineBatchId = batch.Id,
                Type = TransactionType.Dispense,
                QuantityChange = -quantityDeducted, 
                TransactionDate = DateTime.UtcNow,
                ReferenceNote = "System Dispense" 
            };
            _context.InventoryTransactions.Add(transaction);
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {   
            throw new Exception("Inventory was modified by another user while processing this request. Please try dispensing again.");
        }
    }
}