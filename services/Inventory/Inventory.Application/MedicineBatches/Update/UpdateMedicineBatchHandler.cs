using Inventory.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.MedicineBatches.Update;

internal sealed class UpdateMedicineBatchCommandHandler 
    : IRequestHandler<UpdateMedicineBatchCommand, Guid>
{
    private readonly IInventoryDbContext _context;

    public UpdateMedicineBatchCommandHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(UpdateMedicineBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch == null)
        {
            throw new Exception($"Không tìm thấy lô hàng với ID: {request.BatchId}");
        }

        batch.BatchNumber = request.BatchNumber;
        batch.ExpiryDate = request.ExpiryDate.ToUniversalTime();
        batch.OriginalQuantity = request.Quantity;

        _context.Batches.Update(batch);
        await _context.SaveChangesAsync(cancellationToken);

        return batch.Id;
    }
}