using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.Medicines;
using Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Medicines.Checkout;

public class CreatePendingSaleCommandHandler : IRequestHandler<CreatePendingSaleCommand, CreatePendingSaleResponseDTO>
{
    private readonly IInventoryDbContext _context;
    private readonly ITenantService _tenantService;

    public CreatePendingSaleCommandHandler(IInventoryDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<CreatePendingSaleResponseDTO> Handle(CreatePendingSaleCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                SaleDate = DateTime.UtcNow,
                SaleStatus = SaleStatus.Pending,
                ProcessedBy = _tenantService.GetCurrentUserId(),
                TotalAmount = 0,
                Items = new List<SaleItem>()
            };

            foreach (var item in request.Items)
            {
                var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == item.MedicineId, cancellationToken);

                if (medicine == null)
                    throw new Exception($"Medicine with ID {item.MedicineId} not found.");

                var batches = await _context.Batches.Where(b => b.MedicineId == medicine.Id && b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow).OrderBy(b => b.ExpiryDate).ToListAsync(cancellationToken);

                int totalAvailableQuantity = batches.Sum(b => b.CurrentQuantity);
                if (totalAvailableQuantity < item.Quantity)
                    throw new Exception($"Insufficient stock for {medicine.Name}. Available: {totalAvailableQuantity}, Requested: {item.Quantity}.");

                decimal unitPrice = batches.FirstOrDefault()?.PurchasePrice ?? 0;
                if (unitPrice == 0)
                    throw new Exception($"Unable to determine unit price for {medicine.Name}.");

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

            return new CreatePendingSaleResponseDTO
            {
                OrderId = sale.Id.ToString(),
                FullName = _tenantService.GetCurrentUserFullName(),
                Amount = sale.TotalAmount,
                OrderInfo = $"Payment for order {sale.Id}"
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
