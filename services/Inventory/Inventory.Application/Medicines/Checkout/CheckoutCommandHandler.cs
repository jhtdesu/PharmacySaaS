using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Medicines.Checkout;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, string>
{
    private readonly IInventoryDbContext _context;
    private readonly ITenantService _tenantService;

    public CheckoutCommandHandler(IInventoryDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<string> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                ReceiptNumber = $"REC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}",
                SaleDate = DateTime.UtcNow,
                ProcessedBy = _tenantService.GetCurrentTenantId(),
                TotalAmount = 0,
                Items = new List<SaleItem>()
            };

            foreach (var item in request.Items)
            {
                var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == item.MedicineId, cancellationToken);

                if (medicine == null)
                    throw new Exception($"Medicine with ID {item.MedicineId} not found.");

                var batches = await _context.Batches
                    .Where(b => b.MedicineId == medicine.Id && b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow)
                    .OrderBy(b => b.ExpiryDate)
                    .ToListAsync(cancellationToken);

                int remainingToFulfill = item.Quantity;
                decimal unitPrice = 0;

                foreach (var batch in batches)
                {
                    if (remainingToFulfill == 0) break;

                    int quantityToDeduct = Math.Min(batch.CurrentQuantity, remainingToFulfill);
                    if (unitPrice == 0)
                        unitPrice = batch.PurchasePrice; 
                    batch.CurrentQuantity -= quantityToDeduct;
                    remainingToFulfill -= quantityToDeduct;
                }

                if (remainingToFulfill > 0)
                {
                    throw new Exception($"Insufficient stock for {medicine.Name}. Short by {remainingToFulfill}.");
                }

                var subTotal = item.Quantity * unitPrice;

                sale.Items.Add(new SaleItem
                {
                    Id = Guid.NewGuid(),
                    MedicineId = medicine.Id,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice, 
                    SubTotal = subTotal
                });

                sale.TotalAmount += subTotal;
            }

            _context.Sales.Add(sale);
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