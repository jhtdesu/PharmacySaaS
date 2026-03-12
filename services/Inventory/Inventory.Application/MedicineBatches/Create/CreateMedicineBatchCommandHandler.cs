using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.MedicineBatches.Create;

internal sealed class CreateMedicineBatchCommandHandler 
    : IRequestHandler<CreateMedicineBatchCommand, Guid>
{
    private readonly IInventoryDbContext _context;

    public CreateMedicineBatchCommandHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateMedicineBatchCommand request, CancellationToken cancellationToken)
    {
        var medicineExists = await _context.Medicines
            .AnyAsync(m => m.Id == request.MedicineId, cancellationToken);

        if (!medicineExists)
        {
            // Tạm thời ném ra Exception, sau này chúng ta sẽ làm chuẩn hơn với Result Pattern
            throw new Exception($"Không tìm thấy thuốc với ID: {request.MedicineId}");
        }

        var batch = new MedicineBatch
        {
            Id = Guid.NewGuid(),
            MedicineId = request.MedicineId,
            BatchNumber = request.BatchNumber,
            ImportDate = DateTime.Now.ToUniversalTime(),
            ExpiryDate = request.ExpiryDate.ToUniversalTime(), 
            OriginalQuantity = request.Quantity,
            CurrentQuantity = request.Quantity,
        };

        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            MedicineId = batch.MedicineId,
            MedicineBatchId = batch.Id,
            Type = TransactionType.Receive, 
            QuantityChange = request.Quantity,
            TransactionDate = DateTime.UtcNow.ToUniversalTime(),
            ReferenceNote = $"Stock received for new batch: {batch.BatchNumber}"
        };

        _context.Batches.Add(batch);
        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return batch.Id;
    }
}