using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Models;
using Inventory.Domain.Entities;
using Inventory.Application.Common.Interfaces;

namespace Inventory.Application.MedicineBatches.Commands.AdjustStock;

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, BaseResponse<bool>>
{
    private readonly IInventoryDbContext _context;

    public AdjustStockCommandHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<BaseResponse<bool>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        if (request.QuantityToDeduct <= 0)
            return new BaseResponse<bool>("Quantity to deduct must be greater than zero.");

        var batch = await _context.Batches
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch == null)
            return new BaseResponse<bool>("Batch not found.");

        if (batch.CurrentQuantity < request.QuantityToDeduct)
            return new BaseResponse<bool>("Cannot deduct more stock than is currently available in this batch.");

        batch.CurrentQuantity -= request.QuantityToDeduct;

        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            MedicineId = batch.MedicineId,
            MedicineBatchId = batch.Id,
            Type = TransactionType.Adjustment, 
            QuantityChange = -request.QuantityToDeduct,
            TransactionDate = DateTime.UtcNow,
            ReferenceNote = $"Stock Adjustment: {request.Reason}"
        };

        _context.InventoryTransactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);

        return new BaseResponse<bool>(true, "Stock adjusted successfully.");
    }
}