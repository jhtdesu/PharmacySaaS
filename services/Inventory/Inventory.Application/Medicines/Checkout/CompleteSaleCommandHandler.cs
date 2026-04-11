using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Medicines.Checkout;

public class CompleteSaleCommandHandler : IRequestHandler<CompleteSaleCommand, string>
{
    private readonly IInventoryDbContext _context;

    public CompleteSaleCommandHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(CompleteSaleCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var sale = await _context.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken);

            if (sale == null)
                throw new Exception($"Sale with ID {request.SaleId} not found.");

            if (sale.SaleStatus != SaleStatus.Pending)
                throw new Exception($"Sale is not in Pending status. Current status: {sale.SaleStatus}.");

            foreach (var item in sale.Items)
            {
                var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == item.MedicineId, cancellationToken);

                if (medicine == null)
                    throw new Exception($"Medicine with ID {item.MedicineId} not found.");

                var batches = await _context.Batches.Where(b => b.MedicineId == medicine.Id && b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow).OrderBy(b => b.ExpiryDate).ToListAsync(cancellationToken);

                int remainingToFulfill = item.Quantity;

                foreach (var batch in batches)
                {
                    if (remainingToFulfill == 0) break;

                    int quantityToDeduct = Math.Min(batch.CurrentQuantity, remainingToFulfill);
                    batch.CurrentQuantity -= quantityToDeduct;
                    remainingToFulfill -= quantityToDeduct;
                }

                if (remainingToFulfill > 0)
                {
                    throw new Exception($"Insufficient stock for {medicine.Name}. Short by {remainingToFulfill}.");
                }
            }

            sale.SaleStatus = SaleStatus.Completed;

            _context.Sales.Update(sale);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return sale.ReceiptNumber;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
